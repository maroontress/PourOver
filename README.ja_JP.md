# PourOver

PourOverはコマンドラインツールで、CSVファイルに格納された複数の言語で表現された文字列リソースに対して、各文字列に組み込まれたトークンが適切かどうかを言語間で比較して診断します。

## CSVファイルの構造

CSVファイルはUTF-8でエンコードされ、1行目はヘッダーになります。ヘッダーの一番左のフィールドは使用されません。それ以外のフィールドは、各列の言語名を記載します。ヘッダーは診断の対象ではありませんが、一番左のフィールドを除き、フィールドの内容を診断メッセージで使用します。

ヘッダーに続く、二行目以降の行は、メッセージのIDと、各言語毎の文字列になります。次のような内容の文字列リソースを例とします:

| ID      | `English`    | `Japanese`   |
| :--     | :--          | :--          |
| `HELLO` | `Hello`      | `こんにちは`  |
| `BYE`   | `Bye`        | `さようなら`  |

CSVファイルは以下のようになります:

```plaintext
ID,English,Japanese
HELLO,Hello,こんにちは
BYE,Bye,さようなら
```

## トークン

トークンは文字列リソースに組み込まれたプレイスホルダーで、ブレース（`{`と`}`）で囲まれた文字列です。例えば、次のような内容の文字列リソースがあるとします:

| ID     | `English`             | `Japanese`           |
| :--    | :--                   | :--                  |
| `TIME` | `It's {hour} o'clock` | `{hour}時です`        |
| `DEAR` | `Dear {name},`        | `拝啓 {name} さん、`  |

`TIME`の文字列リソースでは、どの言語においてもトークン`{hour}`が組み込まれ、表示する際に`{hour}`は現在の時刻の（時分秒の）時に置き換えられます。また、`DEAR`では、同様に`{name}`が組み込まれ、表示する際に`{name}`は人名に置き換えられます。

文字列リソースはトークンを複数含むことができます。

## 診断メッセージ

診断メッセージは次のような形式になります:

> _ファイル名_ `:` _行番号_ `: ` _ID_ `: ` _診断メッセージ_

## トークンの診断

多くの場合、トークンは言語が変わっても、出現する個数は変わりません（出現順序が異なることはあります）。例えば、ある文字列リソースで、言語 _A_ がトークン`{foo}`と`{bar}`を含み、言語 _B_ がトークン`{foo}`と`{baz}`を含むなら、何か間違っている可能性があります（もちろん、例外的に間違っていない場合もあります）。このように、簡単なヒューリスティックスを使って、トークンを診断することが可能です。

次のト－クンの診断があります:

- TypeNumberMismatch
- StrayToken
- FrequencyMismatch

### TypeNumberMismatch

ある文字列リソースで、トークンの種類の数が言語によって異なる場合に報告します。例えば、言語 _A_ は`{foo}`、`{bar}`、`{baz}`の三種類のトークンを含み、言語 _B_ は`{foo}`、`{bar}`の二種類のトークンしか含まない場合が該当します。同じトークンが複数回出現しても、種類としては1つとして数えます。

例えば、次のような文字列リソースを考えます:

| ID         | `English`           | `Japanese`              |
| :--        | :--                 | :--                     |
| `EXAMPLE1` | `Hello {foo} {bar}` | `こんにちは {foo}`       |
| `EXAMPLE2` | `Bye {foo} {bar}`   | `さようなら {foo} {foo}` |

このとき、次のような診断を出力します（ロケールが英語の場合）:

```plain
file.csv:2: EXAMPLE1: The number of unique tokens is different: 'English' has 2 token(s) but 'Japanese' has 1 token(s).
file.csv:3: EXAMPLE2: The number of unique tokens is different: 'English' has 2 token(s) but 'Japanese' has 1 token(s).
```

なお、このTypeNumberMismatchが診断された場合、そのフィールドに対しては以降の診断を実施しません。

### StrayToken

ある文字列リソースで、トークンの種類が言語によって異なる場合に報告します。例えば、言語 _A_ は`{foo}`、`{bar}`のトークンを含み、言語 _B_ は`{foo}`、`{baz}`のトークンを含む場合が該当します。

例えば、次のような文字列リソースを考えます:

| ID         | `English`           | `Japanese`              |
| :--        | :--                 | :--                     |
| `EXAMPLE1` | `Hello {foo} {bar}` | `こんにちは {foo} {baz}` |

このとき、次のような報告を出力します（ロケールが英語の場合）:

```plaintext
file.csv:2: EXAMPLE1: Token {bar} appears only in 'English'.
file.csv:2: EXAMPLE1: Token {baz} appears only in 'Japanese'.
```

なお、このStrayTokenの診断を報告した場合は、以降の診断を実施しません。

### FrequencyMismatch

ある文字列リソースで、特定のトークンの出現回数が言語によって異なる場合に報告します。例えば、言語 _A_ はトークン`{foo}`を一つだけ含み、言語 _B_ はトークン`{foo}`を二つ含む場合が該当します。

例えば、次のような文字列リソースを考えます:

| ID         | `English`           | `Japanese`              |
| :--        | :--                 | :--                     |
| `EXAMPLE1` | `Hello {foo} {foo}` | `こんにちは {foo}`       |

このとき、次のような報告を出力します（ロケールが英語の場合）:

```plaintext
file.csv:2: EXAMPLE1: Token {foo} appears 2 time(s) in 'English' but appears 1 time(s) in 'Japanese'.
```

## そのほかの診断

そのほか次の診断があります:

- InvalidToken
- DuplicateID

### InvalidToken

フィールド中のブレース（`{`と`}`）の対応が間違っていて、トークンをパースできないときに報告します。例えば、次のような文字列リソースを考えます:

| ID         | `English`              | `Japanese`              |
| :--        | :--                    | :--                     |
| `EXAMPLE1` | `Good morning {foo}`   | `おはようございます {foo` |
| `EXAMPLE2` | `Good afternoon {foo}` | `こんにちは foo}`        |
| `EXAMPLE3` | `Good evening {f{oo}`  | `こんばんは {foo}`       |

このとき、次のような報告を出力します（ロケールが英語の場合）:

```plaintext
file.csv:2: EXAMPLE1: ’Japanese’ has an invalid token: Missing a closing brace ('}')
file.csv:3: EXAMPLE2: ’Japanese’ has an invalid token: Missing an opening brace ('{')
file.csv:4: EXAMPLE3: ’English’ has an invalid token: Token containing an opening brace ('{')
```

なお、このInvalidTokenの診断を報告した場合は、そのフィールドはトークンが含まれないものとみなして以降の診断を続行します。

### DuplicateID

CSVファイルでIDの重複があると報告します。例えば、次のような文字列リソースを考えます:

| ID         | `English`           | `Japanese`              |
| :--        | :--                 | :--                     |
| `EXAMPLE1` | `Hello {foo}`       | `こんにちは {foo}`       |
| `EXAMPLE1` | `Bye {foo}`         | `さようなら {foo}`       |

このとき、次のような報告を出力します（ロケールが英語の場合）:

```plaintext
file.csv:3: EXAMPLE1: This ID already appeared at line 2.
```

## Requirements

- [.NET Core 3.1 Runtime (Runtime 3.1)][dotnet-core-runtime]

## Get started

PourOverは[NuGetパッケージ][pourover.globaltool]で利用可能です。次のようにインストールできます:

```plaintext
dotnet tool install -g PourOver.GlobalTool
```

## Synopsis

> `PourOver` [`-L` _CULTURE_] [`-hbvV`] [`--`] _FILE_.csv

## Description

_FILE_.csvは前述した構造のCSVファイルです。

オプションは次のようになります:

| | Option | | Description |
|---:|:---|:---|:---|
| `-L`, | `--culture` | _CULTURE_ | カルチャーを指定します（例: `en_US`） |
| `-h`, | `--help` | | ヘルプメッセージを表示して終了します |
| `-b`, | `--ignore-blank` | | 空欄のフィールドを無視します |
| `-v`, | `--verbose` | | 出力が冗舌になります |
| `-V`, | `--version` | | バージョンを出力して終了します |

### Exit status

PourOverは正常に終了した場合、終了ステータス0で終了します（診断の有無とは無関係です）。CSVファイルのフォーマットが壊れているなどのエラーが検出された場合は、正の整数で終了します。

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
