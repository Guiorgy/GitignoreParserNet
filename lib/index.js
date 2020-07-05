/**
 * Compile the given `.gitignore` content (not filename!)
 * and return an object with `accepts`, `denies` and `maybe` methods.
 * These methods each accepts a single filename and determines whether
 * they are acceptable or unacceptable according to the `.gitignore` definition.
 *
 *
 * @param  {String} content The `.gitignore` content to compile.
 * @return {Object}         The helper object with methods that operate on the compiled content.
 */
exports.compile = function (content) {
  var parsed = exports.parse(content),
      positives = parsed[0],
      negatives = parsed[1];
  return {
    accepts: function (input) {
      if (input[0] === '/') input = input.slice(1);
      return negatives[0].test(input) || !positives[0].test(input);
    },
    denies: function (input) {
      if (input[0] === '/') input = input.slice(1);
      return !(negatives[0].test(input) || !positives[0].test(input));
    },
    maybe: function (input) {
      if (input[0] === '/') input = input.slice(1);
      return negatives[1].test(input) || !positives[1].test(input);
    }
  };
};

/**
 * Parse the given `.gitignore` content and return an array
 * containing two further arrays - positives and negatives.
 * Each of these two arrays in turn contains two regexps, one
 * strict and one for 'maybe'.
 *
 * @param  {String} content  The content to parse,
 * @return {Array[]}         The parsed positive and negatives definitions.
 */
exports.parse = function (content) {
  return content.split('\n')
  .map(function (line) {
    line = line.trim();
    return line;
  })
  .filter(function (line) {
    return line && line[0] !== '#';
  })
  .reduce(function (lists, line) {
    var isNegative = line[0] === '!';
    if (isNegative) {
      line = line.slice(1);
    }
    if (isNegative) {
      lists[1].push(line);
    }
    else {
      lists[0].push(line);
    }
    return lists;
  }, [[], []])
  .map(function (list) {
    return list
    .sort()
    .map(prepareRegexes)
    .reduce(function (list, prepared) {
      list[0].push(prepared[0]);
      list[1].push(prepared[1]);
      return list;
    }, [[], [], []]);
  })
  .map(function (item) {
    // don't need submatches, hence we use should use non-capturing `(?:...)` grouping regexes: 
    // those are generally faster than their submatch-capturing brothers:
    return [
      item[0].length > 0 ? new RegExp('(?:' + item[0].join(')|(?:') + ')') : new RegExp('$^'),
      item[1].length > 0 ? new RegExp('(?:' + item[1].join(')|(?:') + ')') : new RegExp('$^')
    ]
  });
};

function prepareRegexes (pattern) {
  return [
    // exact regex
    prepareRegexPattern(pattern),
    // partial regex
    preparePartialRegex(pattern)
  ];
};

function prepareRegexPattern (pattern) {
  // https://git-scm.com/docs/gitignore#_pattern_format
  // 
  // * ... 
  // 
  // * If there is a separator at the beginning or middle (or both) of the pattern, 
  //   then the pattern is relative to the directory level of the particular 
  //   .gitignore file itself. 
  //   Otherwise the pattern may also match at any level below the .gitignore level.
  //   
  // * ... 
  // 
  // * For example, a pattern `doc/frotz/` matches `doc/frotz` directory, but 
  //   not `a/doc/frotz` directory; however `frotz/` matches `frotz` and `a/frotz`
  //   that is a directory (all paths are relative from the .gitignore file).
  //   
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
  }
  // keep character ranges intact:
  const rangeRe = /^((?:[^\[\\]|(?:\\.))*)\[((?:[^\]\\]|(?:\\.))*)\]/;   
  // ^ could have used the 'y' sticky flag, but there's some trouble with infine loops inside
  //   the matcher below then...
  let match;

  while ((match = rangeRe.exec(pattern)) !== null) {
    if (match[1].includes('/')) {
      rooted = true;
      // ^ cf. man page:
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
      rooted = true;
      // ^ cf. man page:
      // 
      //   If there is a separator at the beginning or middle (or both) 
      //   of the pattern, then the pattern is relative to the directory 
      //   level of the particular .gitignore file itself. Otherwise 
      //   the pattern may also match at any level below the .gitignore level.
    }
    re += transpileRegexPart(pattern);
  }

  if (rooted) {
    re = '^' + re;
  }
  if (directory) {
    re += '\\/';
  }
  return re;
}

function preparePartialRegex (pattern) {
  return pattern
  .split('/')
  .map(function (item, index) {
    if (index)
      return '(?:[\\/]?(?:' + prepareRegexPattern(item) + '\\b|$))';
    else
      return '(?:' + prepareRegexPattern(item) + '\\b)';
  })
  .join('');
}

function transpileRegexPart(re) {
return re
      // unescape for these will be escaped again in the subsequent `.replace(...)`, 
      // whether they were escaped before or not:
      .replace(/\\(.)/g, "$1")   
      // escape special regex characters:
      .replace(/[\-\[\]\/\{\}\(\)\+\.\\\^\$\|]/g, "\\$&")
      .replace(/\?/g, '[^\\/]')
      .replace(/\*\*/g, '.+')
      .replace(/\*/g, '[^\\/]+');
}
