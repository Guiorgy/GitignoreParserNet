
/* eslint-env mocha, es6 */

let LIB = require('../');
let fs = require('fs');
let path = require('path');
let assert = require('assert');
let testgenerator = require('@gerhobbelt/markdown-it-testgen');


let FIXTURE = fs.readFileSync(path.join(__dirname, '/./.gitignore-fixture'), 'utf8');
let NO_NEGATIVES_FIXTURE = fs.readFileSync(path.join(__dirname, '/./.gitignore-no-negatives'), 'utf8');

describe('gitignore parser', function () {
  describe('parse()', function () {
    it('should parse some content', function () {
      let parsed = LIB.parse(FIXTURE);
      assert.strictEqual(parsed.length, 2);
    });

    it('should accept ** wildcards', function () {
      let parsed = LIB.parse('a/**/b');
      assert.deepEqual(parsed, [
        [
          /(?:^\/a(?:\/|(?:\/.+\/))b(?:$|\/))/,
          [
            /^\/a(?:\/|(?:\/.+\/))b(?:$|\/)/
          ]
        ],
        [
          /$^/,
          []
        ]
      ]
      );
    });

    it('should accept * wildcards', function () {
      let parsed = LIB.parse('a/b*c');
      assert.deepEqual(parsed, [
        [
          /(?:^\/a\/b[^\/]*c(?:$|\/))/,
          [
            /^\/a\/b[^\/]*c(?:$|\/)/
          ]
        ],
        [
          /$^/,
          []
        ]
      ]
      );
    });

    it('should accept ? wildcards', function () {
      let parsed = LIB.parse('a/b?');
      assert.deepEqual(parsed, [
        [
          /(?:^\/a\/b[^\/](?:$|\/))/,
          [
            /^\/a\/b[^\/](?:$|\/)/
          ]
        ],
        [
          /$^/,
          []
        ]
      ]
      );
    });

    it('should correctly encode literals', function () {
      let parsed = LIB.parse('a/b.c[d](e){f}\\slash^_$+$$$');
      assert.deepEqual(parsed, [
        [
          /(?:^\/a\/b\.c[d]\(e\)\{f\}slash\^_\$\+\$\$\$(?:$|\/))/,
          [
            /^\/a\/b\.c[d]\(e\)\{f\}slash\^_\$\+\$\$\$(?:$|\/)/
          ]
        ],
        [
          /$^/,
          []
        ]
      ]
      );
    });

    it('should correctly parse character ranges', function () {
      let parsed = LIB.parse('a[c-z$].[1-9-][\\[\\]A-Z]-\\[...]');
      assert.deepEqual(parsed, [
        [
          /(?:\/a[c-z$]\.[1-9-][\[\]A-Z]\-\[\.\.\.\](?:$|\/))/,
          [
            /\/a[c-z$]\.[1-9-][\[\]A-Z]\-\[\.\.\.\](?:$|\/)/
          ]
        ],
        [
          /$^/,
          []
        ]
      ]
      );
    });

    it('should treat "/a" as rooted, while "b" should match anywhere', function () {
      // see bullet 6 of https://git-scm.com/docs/gitignore#_pattern_format
      //
      // If there is a separator at the beginning or middle (or both) of the pattern,
      // then the pattern is relative to the directory level of the particular
      // .gitignore file itself. Otherwise the pattern may also match at any level
      // below the .gitignore level.
      let parsed = LIB.parse('/a\nb');
      assert.deepEqual(parsed, [
        [
          /(?:^\/a(?:$|\/))|(?:\/b(?:$|\/))/,
          [
            /^\/a(?:$|\/)/,
            /\/b(?:$|\/)/
          ]
        ],
        [
          /$^/,
          []
        ]
      ]
      );
    });

    it('should correctly transpile rooted and relative path specs', function () {
      let parsed = LIB.parse('a/b\n/c/d\ne/\nf');
      assert.deepEqual(parsed, [
        [
          /(?:^\/c\/d(?:$|\/))|(?:^\/a\/b(?:$|\/))|(?:\/e\/)|(?:\/f(?:$|\/))/,
          [
            /^\/c\/d(?:$|\/)/,
            /^\/a\/b(?:$|\/)/,
            /\/e\//,
            /\/f(?:$|\/)/
          ]
        ],
        [
          /$^/,
          []
        ]
      ]
      );
    });
  });

  describe('compile()', function () {
    let ignore, gitignoreNoNegatives;

    // gitignore file contents
    // -----------------------
    //
    // # This is a comment in a .gitignore file!
    // /node_modules
    // *.log
    //
    // # Ignore this nonexistent file
    // /nonexistent
    //
    // # Do not ignore this file
    // !/nonexistent/foo
    //
    // # Ignore some files
    //
    // /baz
    //
    // /foo/*.wat
    //
    // # Ignore some deep sub folders
    // /othernonexistent/**/what
    //
    // # Ignore deep folders with more than one wildcard
    // */**/__MACOSX/**/*
    //
    // # Unignore some other sub folders
    // !/othernonexistent/**/what/foo
    //
    // *.swp
    //

    beforeEach(function () {
      gitignore = LIB.compile(FIXTURE);
      gitignoreNoNegatives = LIB.compile(NO_NEGATIVES_FIXTURE);
    });

    function gitignoreAccepts(str, expects) {
      assert.strictEqual(gitignore.accepts(str, expects), expects, `expected '${str}' to be ${ expects ? 'accepted' : 'rejected' }`);
    }
    function gitignoreNoNegativesAccepts(str, expects) {
      assert.strictEqual(gitignoreNoNegatives.accepts(str, expects), expects, `expected '${str}' to be ${ expects ? 'accepted' : 'rejected' } when we use gitignoreNoNegatives`);
    }

    describe('accepts()', function () {
      it('should accept the given filenames', function () {
        gitignoreAccepts('test/index.js', true);
        gitignoreAccepts('wat/test/index.js', true);
        gitignoreNoNegativesAccepts('test/index.js', true);
        gitignoreNoNegativesAccepts('node_modules.json', true);
      });

      it('should not accept the given filenames', function () {
        gitignoreAccepts('test.swp', false);
        gitignoreAccepts('foo/test.swp', false);
        gitignoreAccepts('node_modules/wat.js', false);
        gitignoreAccepts('foo/bar.wat', false);
        gitignoreNoNegativesAccepts('node_modules/wat.js', false);
      });

      it('should not accept the given directory', function () {
        gitignoreAccepts('nonexistent', false);
        gitignoreAccepts('nonexistent/bar', false);
        gitignoreNoNegativesAccepts('node_modules', false);
      });

      it('should accept unignored files in ignored directories', function () {
        gitignoreAccepts('nonexistent/foo', true);
      });

      it('should accept nested unignored files in ignored directories', function () {
        gitignoreAccepts('nonexistent/foo/wat', true);
      });
    });

    function gitignoreDenies(str, expects) {
      assert.strictEqual(gitignore.denies(str, expects), expects, `expected '${str}' to be ${ expects ? 'denied' : 'accepted' }`);
    }
    function gitignoreNoNegativesDenies(str, expects) {
      assert.strictEqual(gitignoreNoNegatives.denies(str, expects), expects, `expected '${str}' to be ${ expects ? 'denied' : 'accepted' } when we use gitignoreNoNegatives`);
    }

    describe('denies()', function () {
      it('should deny the given filenames', function () {
        gitignoreDenies('test.swp', true);
        gitignoreDenies('foo/test.swp', true);
        gitignoreDenies('node_modules/wat.js', true);
        gitignoreDenies('foo/bar.wat', true);
        gitignoreNoNegativesDenies('node_modules/wat.js', true);
      });

      it('should not deny the given filenames', function () {
        gitignoreDenies('test/index.js', false);
        gitignoreDenies('wat/test/index.js', false);
        gitignoreNoNegativesDenies('test/index.js', false);
        gitignoreNoNegativesDenies('wat/test/index.js', false);
        gitignoreNoNegativesDenies('node_modules.json', false);
      });

      it('should deny the given directory', function () {
        gitignoreDenies('nonexistent', true);
        gitignoreDenies('nonexistent/bar', true);
        gitignoreNoNegativesDenies('node_modules', true);
        gitignoreNoNegativesDenies('node_modules/foo', true);
      });

      it('should not deny unignored files in ignored directories', function () {
        gitignoreDenies('nonexistent/foo', false);
      });

      it('should not deny nested unignored files in ignored directories', function () {
        gitignoreDenies('nonexistent/foo/wat', false);
      });
    });

    function gitignoreIsInspected(str, expects) {
      assert.strictEqual(gitignore.inspects(str, expects), expects, `expected '${str}' to ${ expects ? 'be inspected' : 'NOT be inspected' }`);
    }
    function gitignoreNoNegativesIsInspected(str, expects) {
      assert.strictEqual(gitignoreNoNegatives.inspects(str, expects), expects, `expected '${str}' to ${ expects ? 'be inspected' : 'NOT be inspected' } when we use gitignoreNoNegatives`);
    }

    describe('inspects()', function () {
      it('should return false for directories not mentioned by .gitignore', function () {
        gitignoreIsInspected('lib', false);
        gitignoreIsInspected('lib/foo/bar', false);
        gitignoreNoNegativesIsInspected('lib', false);
        gitignoreNoNegativesIsInspected('lib/foo/bar', false);
      });

      it('should return true for directories explicitly mentioned by .gitignore', function () {
        gitignoreIsInspected('baz', true);
        gitignoreIsInspected('baz/wat/foo', true);
        gitignoreNoNegativesIsInspected('node_modules', true);
      });

      it('should return true for ignored directories that have exceptions', function () {
        gitignoreIsInspected('nonexistent', true);
        gitignoreIsInspected('nonexistent/foo', true);
        gitignoreIsInspected('nonexistent/foo/bar', true);
      });

      it('should return true for non exceptions of ignored subdirectories', function () {
        gitignoreIsInspected('nonexistent/wat', true);
        gitignoreIsInspected('nonexistent/wat/foo', true);
        gitignoreNoNegativesIsInspected('node_modules/wat/foo', true);
      });
    });
  });

  describe('github issues & misc tests', function () {

    function gitignoreAccepts(gitignore, str, expects) {
      assert.strictEqual(gitignore.accepts(str, expects), expects, `expected '${str}' to be ${ expects ? 'accepted' : 'rejected' }`);
    }
    function gitignoreDenies(gitignore, str, expects) {
      assert.strictEqual(gitignore.denies(str, expects), expects, `expected '${str}' to be ${ expects ? 'denied' : 'accepted' }`);
    }

    describe('Test case: a', function () {
      it('should only accept a/2/a', function () {
        const gitignore = LIB.compile(fs.readFileSync(path.join(__dirname, '/a/.gitignore-fixture'), 'utf8'));
        gitignoreAccepts(gitignore, 'a/2/a', true);
        gitignoreAccepts(gitignore, 'a/3/a', false);
      });
    });

    describe('issue #12', function () {
      it('should not fail test A', function () {
        let gitignore = LIB.compile('/ajax/libs/bPopup/*b*');
        gitignoreAccepts(gitignore, '/ajax/libs/bPopup/0.9.0', true);  //output false
      });

      it('should not fail test B', function () {
        let gitignore = LIB.compile('/ajax/libs/jquery-form-validator/2.2');
        gitignoreAccepts(gitignore, '/ajax/libs/jquery-form-validator/2.2.43', true); //output false
      });

      it('should not fail test C', function () {
        let gitignore = LIB.compile('/ajax/libs/punycode/2.0');
        gitignoreAccepts(gitignore, '/ajax/libs/punycode/2.0.0', true); //output false
      });

      it('should not fail test D', function () {
        let gitignore = LIB.compile('/ajax/libs/typescript/*dev*');
        gitignoreAccepts(gitignore, '/ajax/libs/typescript/2.0.6-insiders.20161014', true); //output false
      });
    });

    describe('issue #14', function () {
      it('should not fail test A', function () {
        let gitignore = LIB.compile('node-modules');
        gitignoreDenies(gitignore, 'packages/my-package/node-modules', true);
        gitignoreAccepts(gitignore, 'packages/my-package/node-modules', false);
      });
    });

    describe('when using the `expected` argument, accepts/denies should fire the `diagnose()` callback', function () {
      it('accepts() should fail and call diagnose()', function () {
        let gitignore = LIB.compile('node_modules');
        let count = 0;
        gitignore.diagnose = function () { count++; };

        assert.strictEqual(gitignore.accepts('node_modules', true /* INTENTIONALLY WRONG */), false, 'accepts() call is expected to return FALSE');
        assert.strictEqual(count, 1, 'gitignore.diagnose() should have been invoked once from inside the accepts() call');
        // the next one is matching the expectation, hence no diagnose() call:
        assert.strictEqual(gitignore.accepts('node_modules', false), false, 'accepts() call is expected to return FALSE');
        assert.strictEqual(count, 1, 'gitignore.diagnose() should NOT have been invoked once from inside the accepts() call');

        assert.strictEqual(gitignore.denies('node_modules', false /* INTENTIONALLY WRONG */), true, 'denies() call is expected to return TRUE');
        assert.strictEqual(count, 2, 'gitignore.diagnose() should have been invoked once from inside the denies() call');
        // the next one is matching the expectation, hence no diagnose() call:
        assert.strictEqual(gitignore.denies('node_modules', true), true, 'denies() call is expected to return TRUE');
        assert.strictEqual(count, 2, 'gitignore.diagnose() should NOT have been invoked once from inside the denies() call');

        assert.strictEqual(gitignore.inspects('node_modules', false /* INTENTIONALLY WRONG */), true, 'inspects() call is expected to return TRUE');
        assert.strictEqual(count, 3, 'gitignore.diagnose() should have been invoked once from inside the inspects() call');
        // the next one is matching the expectation, hence no diagnose() call:
        assert.strictEqual(gitignore.inspects('node_modules', true), true, 'inspects() call is expected to return TRUE');
        assert.strictEqual(count, 3, 'gitignore.diagnose() should NOT have been invoked once from inside the inspects() call');
      });
    });
  });


  testgenerator(path.join(__dirname, 'fixtures/gitignore.manpage.txt'), {
    desc: 'gitignore specification / manpage',

    test: function test_gitignore(_it_, title, fixture, options, md, env) {
      let gitignore = LIB.compile(fixture.first.text);

      function mkTest(title, test) {
        let t = test.split('⟹').map((l) => l.trim());
        let expect = t[1].toLowerCase();
        let expectAccept = expect.replace(/^.*(accept|reject).*$/, '$1') === 'accept';
        // 'inspects' expectation: unless overridden, any 'accept' won't have been inspected, while all 'reject's certainly have:
        let expectNoInspection = /inspect/.test(expect) ? /\!inspects|not inspect/.test(expect) : expectAccept;

        it(title, function () {
          assert.strictEqual(gitignore.accepts(t[0], expectAccept), expectAccept, `expected: ${t[0]} ⟹ ${expectAccept ? 'accept' : 'reject'}`);
          assert.strictEqual(gitignore.denies(t[0], !expectAccept), !expectAccept, `expected: ${t[0]} ⟹ ${expectAccept ? 'accept' : 'reject'}`);
          assert.strictEqual(gitignore.inspects(t[0], !expectNoInspection), !expectNoInspection, `expected inspection: ${t[0]} ⟹ ${expectNoInspection ? 'does not inspect (!inspects)' : 'inspects'}`);
        });
      }

      let tests = fixture.second.text.split('\n');
      let line = fixture.second.range[0];
      for (let i = 0; i < tests.length; i++) {
        let test = tests[i].trim();
        // don't do this bit in a .filter() expression as we want to keep our
        // line numbers intact for the real tests:
        if (test.length === 0 || test.startsWith('#')) {
          continue;
        }

        // watch out for closure use inside loop! --> use indirection via mkTest()
        mkTest('line ' + (line + i + 1), test);
      }
    }
  });

});
