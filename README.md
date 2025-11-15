# This is a .NET port of [gitignore-parser](https://github.com/GerHobbelt/gitignore-parser) for node.js by [Ger Hobbelt](https://github.com/GerHobbelt)

A simple yet *complete* [`.gitignore`](https://git-scm.com/docs/gitignore#_pattern_format) parser for .NET.

## Installation

[![NuGet Badge](https://badgen.net/nuget/v/GitignoreParserNet)](https://www.nuget.org/packages/GitignoreParserNet#versions-body-tab)
[![NuGet Badge](https://badgen.net/nuget/dt/GitignoreParserNet)](https://www.nuget.org/stats/packages/GitignoreParserNet?groupby=Version)

`Install-Package GitignoreParserNet -Version 0.2.0.15`

## Features

Supports all features listed in the [GIT SCM gitignore manpage](https://git-scm.com/docs/gitignore):

- handles the `**` *wildcard* anywhere
  - in both the usual usage, e.g. `foo/**/bar`, *and* also in complexes such as `yo/**la/bin`
  - can be used multiple times in a single pattern, e.g. `foo/**/rec**on`
- handles the `*` *wildcard*
- handles the `?` *wildcard*
- handles `[a-z]` style *character ranges*
- understands `!`-prefixed *negated patterns*
- understands `\#`, `\[`, `\\`, etc. filename *escapes*, thus handles patterns like `\#*#` correctly (*hint: this is NOT a comment line!*)
- deals with any *sequence* of positive and negative patterns, like this one from the `.gitignore` manpage:
  
  ```bash
  # exclude everything except directory foo/bar
  /*
  !/foo
  /foo/*
  !/foo/bar
  ```

- handles any empty lines and *`#` comment lines* you feed it

- we're filename agnostic: the *"`.gitignore` file"* does not have to be named `.gitignore` but can be named anything: this parser accepts `.gitignore`-formatted content from anywhere: *you* load the file, *we* do the parsing, *you* feed our `Accepts()` or `Denies()` APIs any filenames / paths you want filtered and we'll tell you if it's a go or a *no go*.

- extra: an additional API is available for those of you who wish to have the *complete and utter `.gitignore` experience*: use our `Inspects(path)` API to know whether the given gitignore filter set did actively filter the given file or did simple allow it to pass through.

  ***Read as**: if the `.gitignore` has a pattern which matches the given file/path, then we will return `true`, otherwise we return `false`.*
  
  Use this in directory trees where you have multiple `.gitignore` files in nested directories and are implementing tooling with `git`-like `.gitignore` behaviour.

## Usage

```cs
static void Main(string[] args)
{
    const string gitignorePath = @"D:\path\to\.gitignore";

    var (accepted, denied) = GitignoreParser.Parse(gitignorePath: gitignorePath, ignoreGitDirectory: true);

    foreach (string file in accepted)
        Console.WriteLine(file);
}
```

### Notes

- As the `.gitignore` spec differentiates between *patterns* such as `foo` and `foo/`, where the latter only matches any **directory** named `foo`, you MUST pass the is-this-a-file-or-a-directory info to us when you invoke any of our `Accepts()`, `Denies()` and `Inspects()` APIs by making sure directory paths have a trailing `/`.

## License

Apache 2, see [LICENSE.md](./LICENSE.md).
