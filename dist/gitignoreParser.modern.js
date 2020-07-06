/*! gitignore-parser 0.1.0-4 https://github.com//GerHobbelt/gitignore-parser @license Apache License, Version 2.0 */

// force to false and smart code compressors can remove the resulting 'dead code':
/**
 * Compile the given `.gitignore` content (not filename!)
 * and return an object with `accepts`, `denies` and `inspects` methods.
 * These methods each accepts a single filename or path and determines whether
 * they are acceptable or unacceptable according to the `.gitignore` definition.
 *
 * @param  {String} content The `.gitignore` content to compile.
 * @return {Object}         The helper object with methods that operate on the compiled content.
 */

function compile(content) {
  let parsed = parse(content),
      positives = parsed[0],
      negatives = parsed[1];
  return {
    /// Helper (which can be overridden by userland code) invoked when
    /// any `accepts()`, `denies()` or `inspects()` fail to help
    /// the developer analyze what is going on inside: some gitignore spec
    /// bits are non-intuitive / non-trivial, after all.
    diagnose: function (query) {
    },
    /// Return TRUE when the given `input` path PASSES the gitignore filters,
    /// i.e. when the given input path is DENIED.
    ///
    /// Notes:
    /// - you MUST postfix a input directory with '/' to ensure the gitignore
    ///   rules can be applied conform spec.
    /// - you MAY prefix a input directory with '/' when that directory is
    ///   'rooted' in the same directory as the compiled .gitignore spec file.
    accepts: function (input, expected) {
      if (input[0] === '/') input = input.slice(1);
      input = '/' + input;
      let acceptRe = negatives;
      let acceptTest = acceptRe.test(input);
      let denyRe = positives;
      let denyTest = denyRe.test(input);
      let returnVal = acceptTest || !denyTest;

      if (expected != null && expected !== returnVal) {
        this.diagnose({
          query: 'accepts',
          input,
          expected,
          acceptRe,
          acceptTest,
          denyRe,
          denyTest,
          combine: '(Accept || !Deny)',
          returnVal
        });
      }

      return returnVal;
    },
    /// Return TRUE when the given `input` path FAILS the gitignore filters,
    /// i.e. when the given input path is ACCEPTED.
    ///
    /// Notes:
    /// - you MUST postfix a input directory with '/' to ensure the gitignore
    ///   rules can be applied conform spec.
    /// - you MAY prefix a input directory with '/' when that directory is
    ///   'rooted' in the same directory as the compiled .gitignore spec file.
    denies: function (input, expected) {
      if (input[0] === '/') input = input.slice(1);
      input = '/' + input;
      let acceptRe = negatives;
      let acceptTest = acceptRe.test(input);
      let denyRe = positives;
      let denyTest = denyRe.test(input); // boolean logic:
      //
      // denies = !accepts =>
      // = !(Accept || !Deny) =>
      // = (!Accept && !!Deny) =>
      // = (!Accept && Deny)

      let returnVal = !acceptTest && denyTest;

      if (expected != null && expected !== returnVal) {
        this.diagnose({
          query: 'denies',
          input,
          expected,
          acceptRe,
          acceptTest,
          denyRe,
          denyTest,
          combine: '(!Accept && Deny)',
          returnVal
        });
      }

      return returnVal;
    },
    /// Return TRUE when the given `input` path is inspected by any .gitignore
    /// filter line.
    ///
    /// You can use this method to help construct the decision path when you
    /// process nested .gitignore files: .gitignore filters in subdirectories
    /// MAY override parent .gitignore filters only when there's actually ANY
    /// filter in the child .gitignore after all.
    ///
    /// Notes:
    /// - you MUST postfix a input directory with '/' to ensure the gitignore
    ///   rules can be applied conform spec.
    /// - you MAY prefix a input directory with '/' when that directory is
    ///   'rooted' in the same directory as the compiled .gitignore spec file.
    inspects: function (input, expected) {
      if (input[0] === '/') input = input.slice(1);
      input = '/' + input;
      let acceptRe = negatives;
      let acceptTest = acceptRe.test(input);
      let denyRe = positives;
      let denyTest = denyRe.test(input); // when any filter 'touches' the input path, it must match,
      // no matter whether it's a deny or accept filter line:

      let returnVal = acceptTest || denyTest;

      if (expected != null && expected !== returnVal) {
        this.diagnose({
          query: 'inspects',
          input,
          expected,
          acceptRe,
          acceptTest,
          denyRe,
          denyTest,
          combine: '(Accept || Deny)',
          returnVal
        });
      }

      return returnVal;
    }
  };
}
/**
 * Parse the given `.gitignore` content and return an array
 * containing positives and negatives.
 * Each of these in turn contains a regexp which will be
 * applied to the 'rooted' paths to test for *deny* or *accept*
 * respectively.
 *
 * @param  {String} content  The content to parse,
 * @return {Array[]}         The parsed positive and negatives definitions.
 */

function parse(content) {
  return content.split('\n').map(function (line) {
    line = line.trim();
    return line;
  }).filter(function (line) {
    return line && line[0] !== '#';
  }).reduce(function (lists, line) {
    let isNegative = line[0] === '!';

    if (isNegative) {
      line = line.slice(1);
    }

    if (isNegative) {
      lists[1].push(line);
    } else {
      lists[0].push(line);
    }

    return lists;
  }, [[], []]).map(function (list) {
    list = list.sort().map(prepareRegexPattern); // don't need submatches, hence we use should use non-capturing `(?:...)` grouping regexes:
    // those are generally faster than their submatch-capturing brothers:

    if (list.length > 0) {
      return new RegExp('(?:' + list.join(')|(?:') + ')');
    } // this regex *won't match a thing*:


    return new RegExp('$^');
  });
}

function prepareRegexPattern(pattern) {
  let re = '';
  let rooted = false;
  let directory = false;

  if (pattern[0] === '/') {
    rooted = true;
    pattern = pattern.slice(1);
  }

  if (pattern[pattern.length - 1] === '/') {
    directory = true;
    pattern = pattern.slice(0, pattern.length - 1);
  } // keep character ranges intact:


  const rangeRe = /^((?:[^\[\\]|(?:\\.))*)\[((?:[^\]\\]|(?:\\.))*)\]/; // ^ could have used the 'y' sticky flag, but there's some trouble with infine loops inside
  //   the matcher below then...

  let match;

  while ((match = rangeRe.exec(pattern)) !== null) {
    if (match[1].includes('/')) {
      rooted = true; // ^ cf. man page:
      //
      //   If there is a separator at the beginning or middle (or both)
      //   of the pattern, then the pattern is relative to the directory
      //   level of the particular .gitignore file itself. Otherwise
      //   the pattern may also match at any level below the .gitignore level.
    }

    re += transpileRegexPart(match[1]);
    re += '[' + match[2] + ']';
    pattern = pattern.slice(match[0].length);
  }

  if (pattern) {
    if (pattern.includes('/')) {
      rooted = true; // ^ cf. man page:
      //
      //   If there is a separator at the beginning or middle (or both)
      //   of the pattern, then the pattern is relative to the directory
      //   level of the particular .gitignore file itself. Otherwise
      //   the pattern may also match at any level below the .gitignore level.
    }

    re += transpileRegexPart(pattern);
  } // prep regexes assuming we'll always prefix the check string with a '/':


  if (rooted) {
    re = '^\\/' + re;
  } else {
    re = '\\/' + re;
  } // cf spec:
  //
  //   If there is a separator at the end of the pattern then the pattern
  //   will only match directories, otherwise the pattern can match
  //   **both files and directories**.                   (emphasis mine)


  if (directory) {
    re += '\\/$';
  } else {
    re += '\\/?$';
  } // regex validation diagnostics: better to check if the part is valid

  return re;
}

function transpileRegexPart(re) {
  return re // unescape for these will be escaped again in the subsequent `.replace(...)`,
  // whether they were escaped before or not:
  .replace(/\\(.)/g, '$1') // escape special regex characters:
  .replace(/[\-\[\]\{\}\(\)\+\.\\\^\$\|]/g, '\\$&').replace(/\?/g, '[^/]').replace(/\/\*\*\//g, '(?:/|(?:/.+/))').replace(/^\*\*\//g, '(?:|(?:.+/))').replace(/\/\*\*$/g, '(?:|(?:/.+))') // `a/**` also accepts `a/` itself
  .replace(/\*\*/g, '.*').replace(/\*/g, '[^/]*').replace(/\//g, '\\/');
}

export { compile, parse };
//# sourceMappingURL=gitignoreParser.modern.js.map
