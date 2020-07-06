# [Gitignore](https://git-scm.com/docs/gitignore#_pattern_format) Parser

A very simple [`.gitignore`](https://git-scm.com/docs/gitignore#_pattern_format) parser for node.js.

[![Build Status](https://img.shields.io/travis/GerHobbelt/gitignore-parser/master.svg?style=flat)](https://travis-ci.org/GerHobbelt/gitignore-parser)
[![NPM version](https://img.shields.io/npm/v/@gerhobbelt/gitignore-parser.svg?style=flat)](https://www.npmjs.org/package/@gerhobbelt/gitignore-parser)
[![Coverage Status](https://img.shields.io/coveralls/GerHobbelt/gitignore-parser/master.svg?style=flat)](https://coveralls.io/r/GerHobbelt/gitignore-parser?branch=master)


## Installation

`npm install @gerhobbelt/gitignore-parser`


## Usage

```js
var parser = require('@gerhobbelt/gitignore-parser'),
    fs = require('fs');

var gitignore = parser.compile(fs.readFileSync('.gitignore', 'utf8'));

gitignore.accepts('LICENSE.md') === true;
gitignore.denies('LICENSE.md') === false;

gitignore.accepts('node_modules/mocha/bin') === false;
gitignore.denies('node_modules/mocha/bin') === true;


var files = [
  '.gitignore',
  '.travis.yml',
  'LICENSE.md',
  'README.md',
  'package.json',
  'lib/index.js',
  'test/index.js',
  'test/mocha.opts',
  'node_modules/mocha/bin/mocha',
  'node_modules/mocha/README.md'
];

// only files that are not gitignored
files.filter(gitignore.accepts);

// only files that *are* gitignored
files.filter(gitignore.denies);
```


## License

Apache 2, see [LICENSE.md](./LICENSE.md).

