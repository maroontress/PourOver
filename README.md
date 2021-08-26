# PourOver

PourOver is a command-line tool that diagnoses string resources stored in CSV
files expressed in multiple languages by comparing the tokens embedded in each
string between the languages.

## Structure of CSV file

The CSV file must be encoded with UTF-8, and the first row must be a header.
The leftmost field in it is unused.  All other ones are assumed to represent
the language name for each column.  It does not affect diagnostics, but
diagnostic messages use the contents of the fields in the header, except for
the leftmost one.

The second and subsequent rows following the header are the ID of the message
and strings for each language.  Consider string resources with the following:

| ID      | `English`    | `Japanese`   |
| :--     | :--          | :--          |
| `HELLO` | `Hello`      | `こんにちは`  |
| `BYE`   | `Bye`        | `さようなら`  |

The CSV file should be like the following:

```plaintext
ID,English,Japanese
HELLO,Hello,こんにちは
BYE,Bye,さようなら
```

## Token

A token is a placeholder embedded in a string resource, which is a string
enclosed in braces (`{` and `}`).  For example, you have string resources as
follows:

| ID     | `English`              | `Japanese`           |
| :--    | :--                    | :--                  |
| `TIME` | `It's {hour} o'clock.` | `{hour}時です。`       |
| `DEAR` | `Dear {name},`         | `拝啓 {name} さん、`   |

The string resource `TIME` should include the token `{hour}` in all languages,
and the token `{hour}` is assumed to be replaced with the hour of the current
time when displayed.  In addition, the string resource `DEAR` also includes the
token `{name}`, replacing `{name}` with a person's name when displayed.

A string resource can contain multiple tokens.

## Diagnostic message

The format of each diagnostic message is as follows:

> _FILENAME_ `:`_LINE_ `: ` _ID_ `: ` _MESSAGE_

It is as follows when you specify the `--verbose` option:

> _FILENAME_ `:`_LINE_ `: ` _ID_ `: (` _DIAGNOSTIC-ID_ `) ` _MESSAGE_

## Token diagnostics

In many cases, the number of tokens in the field does not vary by language.
(However, they may appear in a different order.) For example, in a string
resource, if the one of language _A_ contains tokens `{foo}` and `{bar}`, and
the one of language _B_ does tokens `{foo}` and `{baz}`, something might be
wrong.  (Of course, exceptionally, it might not be.) Thus, we can use simple
heuristics to diagnose tokens.

The token-based diagnostics are as follows:

- TypeNumberMismatch
- StrayToken
- FrequencyMismatch

### TypeNumberMismatch

TypeNumberMismatch arises when the number of token types in a string resource
varies by language.  For example, it does when language _A_ contains three
tokens: `{foo}`, `{bar}`, and `{baz}`, but language _B_ contains only two
tokens: `{foo}`, and `{bar}`.  Note that two or more occurrences of the same
token are considered as one type.

Consider the following string resource:

| ID         | `English`           | `Japanese`              |
| :--        | :--                 | :--                     |
| `EXAMPLE1` | `Hello {foo} {bar}` | `こんにちは {foo}`       |
| `EXAMPLE2` | `Bye {foo} {bar}`   | `さようなら {foo} {foo}` |

It prints out diagnostics such as the following (in English locales):

```plain
file.csv:2: EXAMPLE1: The number of unique tokens is different: 'English' has 2 token(s) but 'Japanese' has 1 token(s).
file.csv:3: EXAMPLE2: The number of unique tokens is different: 'English' has 2 token(s) but 'Japanese' has 1 token(s).
```

If the TypeNumberMismatch arises, PourOver performs no further diagnostics to
that field.

### StrayToken

StrayToken arises when the token types vary by language.  For example, it does
when language _A_ contains two tokens of `{foo}`, `{bar}`, and language _B_
does two ones of `{foo}`, `{baz}`.

Consider the following string resource:

| ID         | `English`           | `Japanese`              |
| :--        | :--                 | :--                     |
| `EXAMPLE1` | `Hello {foo} {bar}` | `こんにちは {foo} {baz}` |

It prints out diagnostics such as the following (in English locales):

```plaintext
file.csv:2: EXAMPLE1: Token {bar} appears only in 'English'.
file.csv:2: EXAMPLE1: Token {baz} appears only in 'Japanese'.
```

If the StrayToken arises, PourOver performs no further diagnostics to that
field.

### FrequencyMismatch

FrequencyMismatch arises when the number of occurrences of a particular token in
a string resource varies by language.  For example, it does when language _A_
contains only one token `{foo}`, and language _B_ does two tokens `{foo}`.

Consider the following string resource:

| ID         | `English`           | `Japanese`              |
| :--        | :--                 | :--                     |
| `EXAMPLE1` | `Hello {foo} {foo}` | `こんにちは {foo}`       |

It prints out diagnostics such as the following (in English locales):

```plaintext
file.csv:2: EXAMPLE1: Token {foo} appears 2 time(s) in 'English' but appears 1 time(s) in 'Japanese'.
```

## Other diagnostics

The other diagnostics are as follows:

- InvalidToken
- DuplicateID

### InvalidToken

InvalidToken arises when braces (`{` and `}`) in a field are mismatched, and
PourOver cannot parse tokens.  For example, consider the following string
resource:

| ID         | `English`              | `Japanese`              |
| :--        | :--                    | :--                     |
| `EXAMPLE1` | `Good morning {foo}`   | `おはようございます {foo` |
| `EXAMPLE2` | `Good afternoon {foo}` | `こんにちは foo}`        |
| `EXAMPLE3` | `Good evening {f{oo}`  | `こんばんは {foo}`       |

It prints out diagnostics such as the following (in English locales):

```plaintext
file.csv:2: EXAMPLE1: ’Japanese’ has an invalid token: Missing a closing brace ('}')
file.csv:3: EXAMPLE2: ’Japanese’ has an invalid token: Missing an opening brace ('{')
file.csv:4: EXAMPLE3: ’English’ has an invalid token: Token containing an opening brace ('{')
```

Note that if InvalidToken arises, PourOver assumes the field to contain no
tokens in further diagnostics.

### DuplicateID

DuplicateID arises when the CSV file contains duplicate identifiers.  For
example, consider the following string resource:


| ID         | `English`           | `Japanese`              |
| :--        | :--                 | :--                     |
| `EXAMPLE1` | `Hello {foo}`       | `こんにちは {foo}`       |
| `EXAMPLE1` | `Bye {foo}`         | `さようなら {foo}`       |

It prints out diagnostics such as the following (in English locales):

```plaintext
file.csv:3: EXAMPLE1: This ID already appeared at line 2.
```

## Requirements

- [.NET Core 3.1 Runtime (Runtime 3.1)][dotnet-core-runtime]

## Get started

PourOver is available as [the NuGet package][pourover.globaltool], so it can be
installed as follows:

```plaintext
dotnet tool install -g PourOver.GlobalTool
```

## Synopsis

> `PourOver` [`-L` _CULTURE_] [`-hbvV`] [`--`] _FILE_.csv

## Description

_FILE_.csv is a CSV file with the structure described above.

Options are as follows:

| | Option | | Description |
|---:|:---|:---|:---|
| `-L`, | `--culture` | _CULTURE_ | Specify culture (e.g., `en-US`) |
| `-h`, | `--help` | | Show help message and exit |
| `-b`, | `--ignore-blank` | | Ignore blank fields |
| `-v`, | `--verbose` | | Be verbose |
| `-V`, | `--version` | | Show version and exit |

### Exit status

PourOver exits with an exit status of 0 if successful (regardless of whether
diagnostics arise or not), and &gt;0 if an error occurs, such as the corrupted
format of the CSV file.

## How to build

### Requirements to build

- Visual Studio 2019 Version 16.10
  or [.NET Core 3.1 SDK (SDK 3.1)][dotnet-core-sdk]

### Build with .NET Core SDK

```plaintext
git clone URL
cd PourOver
dotnet restore
dotnet build
```

### Get test coverage report with Coverlet

```plaintext
dotnet test -p:CollectCoverage=true -p:CoverletOutputFormat=opencover \
        --no-build PourOver.Test
dotnet ANYWHERE/reportgenerator.dll \
        --reports:PourOver.Test/coverage.opencover.xml \
        --targetdir:Coverlet-html
```

### Install PourOver as a Global Tool

```plaintext
cd PourOver.GlobalTool
dotnet pack
dotnet tool install --global --add-source bin/Debug PourOver.GlobalTool
```

[dotnet-core-runtime]:
  https://dotnet.microsoft.com/download/dotnet-core/3.1
[dotnet-core-sdk]:
  https://dotnet.microsoft.com/download/dotnet-core/3.1
[pourover.globaltool]:
  https://www.nuget.org/packages/PourOver.GlobalTool/
