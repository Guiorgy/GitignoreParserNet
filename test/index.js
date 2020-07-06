var LIB = require('../lib');
var fs = require('fs');
var path = require('path');
var assert = require('assert');
var testgenerator = require('@gerhobbelt/markdown-it-testgen');


var FIXTURE = fs.readFileSync(__dirname + '/./.gitignore-fixture', 'utf8');
var NO_NEGATIVES_FIXTURE = fs.readFileSync(__dirname + '/./.gitignore-no-negatives', 'utf8');

describe('gitignore parser', function() {
  describe('parse()', function() {
    it('should parse some content', function() {
      var parsed = LIB.parse(FIXTURE);
      assert.strictEqual(parsed.length, 2);
    });

    it('should accept ** wildcards', function() {
      var parsed = LIB.parse('a/**/b');
      assert.deepEqual(parsed, [
   [
   /(?:^\/a(?:\/|(?:\/.+\/))b\/?$)/,
    /(?:(?:\/a\/?$\b)(?:[\/]?(?:\/.[^\/]*\/?$\b|$))(?:[\/]?(?:\/b\/?$\b|$)))/
    ],
   [
     /$^/,
     /$^/
   ]
 ]
);
    });

    it('should accept * wildcards', function() {
      var parsed = LIB.parse('a/b*c');
      assert.deepEqual(parsed, [
  [
   /(?:^\/a\/b[^\/]*c\/?$)/,
    /(?:(?:\/a\/?$\b)(?:[\/]?(?:\/b[^\/]*c\/?$\b|$)))/
   ],
  [
    /$^/,
    /$^/
  ]
]
);
    });

    it('should accept ? wildcards', function() {
      var parsed = LIB.parse('a/b?');
      assert.deepEqual(parsed, [
  [
   /(?:^\/a\/b[^\/]\/?$)/,
    /(?:(?:\/a\/?$\b)(?:[\/]?(?:\/b[^\/]\/?$\b|$)))/
  ],
  [
    /$^/,
    /$^/
  ]
]
);
    });

    it('should correctly encode literals', function() {
      var parsed = LIB.parse('a/b.c[d](e){f}\\slash^_$+$$$');
      assert.deepEqual(parsed, [
  [
   /(?:^\/a\/b\.c[d]\(e\)\{f\}slash\^_\$\+\$\$\$\/?$)/,
    /(?:(?:\/a\/?$\b)(?:[\/]?(?:\/b\.c[d]\(e\)\{f\}slash\^_\$\+\$\$\$\/?$\b|$)))/
  ],
  [
    /$^/,
    /$^/
  ]
]
);
    });

    it('should correctly parse character ranges', function() {
      var parsed = LIB.parse('a[c-z$].[1-9-][\\[\\]A-Z]-\\[...]');
      assert.deepEqual(parsed, [
  [
    /(?:\/a[c-z$]\.[1-9-][\[\]A-Z]\-\[\.\.\.\]\/?$)/,
    /(?:(?:\/a[c-z$]\.[1-9-][\[\]A-Z]\-\[\.\.\.\]\/?$\b))/
    ],
  [
    /$^/,
    /$^/
  ]
]
);
    });

    it('should treat "/a" as rooted, while "b" should match anywhere', function() {
      // see bullet 6 of https://git-scm.com/docs/gitignore#_pattern_format
      // 
      // If there is a separator at the beginning or middle (or both) of the pattern, 
      // then the pattern is relative to the directory level of the particular 
      // .gitignore file itself. Otherwise the pattern may also match at any level 
      // below the .gitignore level.
      var parsed = LIB.parse('/a\nb');
      assert.deepEqual(parsed, [
 [
   /(?:^\/a\/?$)|(?:\/b\/?$)/,
    /(?:(?:\/\/?$\b)(?:[\/]?(?:\/a\/?$\b|$)))|(?:(?:\/b\/?$\b))/
   ],
  [
    /$^/,
    /$^/
  ]
]
);
    });

    it('should correctly transpile rooted and relative path specs', function() {
      var parsed = LIB.parse('a/b\n/c/d\ne/\nf');
      assert.deepEqual(parsed, [
  [
   /(?:^\/c\/d\/?$)|(?:^\/a\/b\/?$)|(?:\/e\/$)|(?:\/f\/?$)/,
    /(?:(?:\/\/?$\b)(?:[\/]?(?:\/c\/?$\b|$))(?:[\/]?(?:\/d\/?$\b|$)))|(?:(?:\/a\/?$\b)(?:[\/]?(?:\/b\/?$\b|$)))|(?:(?:\/e\/?$\b)(?:[\/]?(?:\/\/?$\b|$)))|(?:(?:\/f\/?$\b))/
   ],
  [
    /$^/,
    /$^/
  ]
]
);
    });
  });

  describe('compile()', function() {
    let ignore, gitignoreNoNegatives;

    beforeEach(function() {
      gitignore = LIB.compile(FIXTURE);
      gitignoreNoNegatives = LIB.compile(NO_NEGATIVES_FIXTURE);
    });

    function gitignoreAccepts(str, expects) {
        assert.strictEqual(gitignore.accepts(str, expects), expects, `expected '${str}' to be ${ expects ? "accepted" : "rejected" }`);
    }
    function gitignoreNoNegativesAccepts(str, expects) {
        assert.strictEqual(gitignore.accepts(str, expects), expects, `expected '${str}' to be ${ expects ? "accepted" : "rejected" } when we use gitignoreNoNegatives`);
    }

    describe('accepts()', function() {
      it('should accept the given filenames', function() {
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

      it('should not accept the given directory', function() {
        gitignoreAccepts('nonexistent', false);
        gitignoreAccepts('nonexistent/bar', false);
        gitignoreNoNegativesAccepts('node_modules', false);
      });

      it('should accept unignored files in ignored directories', function() {
        gitignoreAccepts('nonexistent/foo', true);
      });

      it('should accept nested unignored files in ignored directories', function() {
        gitignoreAccepts('nonexistent/foo/wat', true);
      });
    });

    function gitignoreDenies(str, expects) {
        assert.strictEqual(gitignore.denies(str, expects), expects, `expected '${str}' to be ${ expects ? "denied" : "accepted" }`);
    }
    function gitignoreNoNegativesDenies(str, expects) {
        assert.strictEqual(gitignore.denies(str, expects), expects, `expected '${str}' to be ${ expects ? "denied" : "accepted" } when we use gitignoreNoNegatives`);
    }

    describe('denies()', function () {
      it('should deny the given filenames', function () {
        gitignoreDenies('test.swp', true);
        gitignoreDenies('foo/test.swp', true);
        gitignoreDenies('node_modules/wat.js', true);
        gitignoreDenies('foo/bar.wat', true);
        gitignoreNoNegativesDenies('node_modules/wat.js', true);
      });

      it('should not deny the given filenames', function() {
        gitignoreDenies('test/index.js', false);
        gitignoreDenies('wat/test/index.js', false);
        gitignoreNoNegativesDenies('test/index.js', false);
        gitignoreNoNegativesDenies('wat/test/index.js', false);
        gitignoreNoNegativesDenies('node_modules.json', false);
      });

      it('should deny the given directory', function() {
        gitignoreDenies('nonexistent', true);
        gitignoreDenies('nonexistent/bar', true);
        gitignoreNoNegativesDenies('node_modules', true);
        gitignoreNoNegativesDenies('node_modules/foo', true);
      });

      it('should not deny unignored files in ignored directories', function() {
        gitignoreDenies('nonexistent/foo', false);
      });

      it('should not deny nested unignored files in ignored directories', function() {
        gitignoreDenies('nonexistent/foo/wat', false);
      });
    });

    function gitignoreMaybeOkays(str, expects) {
        assert.strictEqual(gitignore.maybe(str, expects), expects, `expected '${str}' to maybe be ${ expects ? "rejected" : "accepted" }`);
    }
    function gitignoreNoNegativesMaybeOkays(str, expects) {
        assert.strictEqual(gitignore.maybe(str, expects), expects, `expected '${str}' to maybe be ${ expects ? "rejected" : "accepted" } when we use gitignoreNoNegatives`);
    }

    describe('maybe()', function() {
      it('should return true for directories not mentioned by .gitignore', function() {
        gitignoreMaybeOkays('lib', true);
        gitignoreMaybeOkays('lib/foo/bar', true);
        gitignoreNoNegativesMaybeOkays('lib', true);
        gitignoreNoNegativesMaybeOkays('lib/foo/bar', true);
      });

      it('should return false for directories explicitly mentioned by .gitignore', function() {
        gitignoreMaybeOkays('baz', false);
        gitignoreMaybeOkays('baz/wat/foo', false);
        gitignoreNoNegativesMaybeOkays('node_modules', false);
      });

      it('should return true for ignored directories that have exceptions', function() {
        gitignoreMaybeOkays('nonexistent', true);
        gitignoreMaybeOkays('nonexistent/foo', true);
        gitignoreMaybeOkays('nonexistent/foo/bar', true);
      });

      it('should return false for non exceptions of ignored subdirectories', function() {
        gitignoreMaybeOkays('nonexistent/wat', false);
        gitignoreMaybeOkays('nonexistent/wat/foo', false);
        gitignoreNoNegativesMaybeOkays('node_modules/wat/foo', false);
      });
    });
  });

describe('github issues & misc tests', function () {

    function gitignoreAccepts(gitignore, str, expects) {
        assert.strictEqual(gitignore.accepts(str, expects), expects, `expected '${str}' to be ${ expects ? "accepted" : "rejected" }`);
    }
    function gitignoreDenies(gitignore, str, expects) {
        assert.strictEqual(gitignore.denies(str, expects), expects, `expected '${str}' to be ${ expects ? "denied" : "accepted" }`);
    }

  describe('Test case: a', function() {
    it('should only accept a/2/a', function() {
      const gitignore = LIB.compile(fs.readFileSync(__dirname + '/a/.gitignore-fixture', 'utf8'));
      gitignoreAccepts(gitignore, 'a/2/a', true);
      gitignoreAccepts(gitignore, 'a/3/a', false);
    });
  })

  describe('issue #12', function () {
    it('should not fail test A', function () {
            var gitignore = LIB.compile('/ajax/libs/bPopup/*b*');
      gitignoreAccepts(gitignore, '/ajax/libs/bPopup/0.9.0', true);  //output false
    });

    it('should not fail test B', function () {
      var gitignore = LIB.compile('/ajax/libs/jquery-form-validator/2.2');
gitignoreAccepts(gitignore, '/ajax/libs/jquery-form-validator/2.2.43', true); //output false
    });

    it('should not fail test C', function () {
      var gitignore = LIB.compile('/ajax/libs/punycode/2.0');
gitignoreAccepts(gitignore, '/ajax/libs/punycode/2.0.0', true); //output false
    });

    it('should not fail test D', function () {
      var gitignore = LIB.compile('/ajax/libs/typescript/*dev*');
gitignoreAccepts(gitignore, '/ajax/libs/typescript/2.0.6-insiders.20161014', true); //output false
    });
  });

  describe('issue #14', function () {
    it('should not fail test A', function () {
            var gitignore = LIB.compile('node-modules');
      gitignoreDenies(gitignore, 'packages/my-package/node-modules', true);
      gitignoreAccepts(gitignore, 'packages/my-package/node-modules', false);
    });
  });
});


  testgenerator(path.join(__dirname, 'fixtures/gitignore.manpage.txt'), {
    desc: 'gitignore specification / manpage',

    test: function test_gitignore(_it_, title, fixture, options, md, env) {
      var gitignore = LIB.compile(fixture.first.text);

      function mkTest(title, test) {
        let t = test.split('⟹').map((l) => l.trim());
        let expect = t[1].toLowerCase();
            
            it(title, function () {
          assert.strictEqual((gitignore.denies(t[0], expect === 'reject') ? 'reject' : 'accept'), expect, 'expected: ' + test);
        })
      }

      var tests = fixture.second.text.split('\n');
      var line = fixture.second.range[0];
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
  })
  
});