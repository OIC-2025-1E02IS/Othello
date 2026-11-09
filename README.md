# オセロ（コンソールアプリ + Windows Forms）

[![CI](https://github.com/OIC-2025-1E02IS/Othello/actions/workflows/ci.yml/badge.svg)](https://github.com/OIC-2025-1E02IS/Othello/actions/workflows/ci.yml)

Visual Studio の「コンソール アプリ」として作ったプロジェクトから、**Windows Forms の GUI ウィンドウ**を開いて遊ぶオセロです。
後ろのコンソール画面には、置いた場所・ひっくり返した枚数・パス・勝敗などのログが流れます。

授業「AI駆動開発の実践」の課題として、生成 AI と仕様を相談し、実装・テスト・CI まで行いました。

## 遊び方

1. Visual Studio で `Othello.sln` を開き、`Othello` をスタートアップにして実行（F5）
2. ウィンドウの盤面で、**白い点のマスをクリック**すると石を置けます
3. 相手の石をはさむと自分の色にひっくり返ります
4. 置ける場所がないときは自動でパス。両方置けなくなったら終了し、石が多い方の勝ち

| 操作 | 内容 |
| --- | --- |
| マスをクリック | 石を置く（マウスを乗せると色が変わる） |
| 新しいゲーム | 選んだ対戦モードで最初から |
| 待った | 1手戻す（CPU 戦ではあなたの番まで戻る） |
| 対戦モード | 人 vs 人 ／ 人 vs CPU（あなたは黒・先手） |
| メニュー | ゲーム（新しいゲーム・終了）、ヘルプ（遊び方） |

CPU は「一番多くひっくり返せる場所」を選ぶ、かんたんな強さです。

## コンソールなのに GUI になるしくみ

`Othello/Othello.csproj` で次のように設定しています。

```xml
<OutputType>Exe</OutputType>                       <!-- コンソールアプリのまま -->
<TargetFramework>net8.0-windows</TargetFramework>  <!-- Windows 専用にする -->
<UseWindowsForms>true</UseWindowsForms>            <!-- Windows Forms を使う -->
```

`Program.Main` でコンソールにログを出したあと、`Application.Run(new MainForm(...))` でウィンドウを開きます。

## 構成

```
Othello.sln
├─ Othello/            起動プロジェクト（コンソール + Windows Forms の画面）
│   ├─ Program.cs          起動・コンソールログ
│   ├─ MainForm.cs         画面全体（パネル・ボタン・メニュー・CPU の手番）
│   ├─ BoardControl.cs     盤面の描画とクリック
│   └─ ConsoleGameLogger.cs
├─ Othello.Core/       ルール（画面に依存しない）
│   ├─ Board.cs            盤面・合法手・反転
│   ├─ Game.cs             手番・パス・終了・待った・勝敗
│   ├─ CpuPlayer.cs        CPU の手の選び方
│   ├─ Position.cs / Stone.cs / MoveResult.cs
├─ Othello.Tests/      xUnit テスト（正常系・異常系・境界値）
├─ .github/workflows/ci.yml
└─ rules.md            実装ルール
```

## ビルドとテスト（コマンド）

```
dotnet build Othello.sln
dotnet test Othello.sln
```

Visual Studio では「テスト」→「テスト エクスプローラー」からも実行できます。

## CI（GitHub Actions）

push と Pull Request のたびに、Windows 環境で次を自動で行います。

1. ビルド
2. テスト（結果は `test-results` として保存）
3. 実行ファイルを作成し、`Othello-app` として保存（Actions の実行結果画面からダウンロード可能。実行には .NET 8 デスクトップ ランタイムが必要）
