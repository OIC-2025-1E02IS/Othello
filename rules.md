# 実装ルール

## 構成
- `Othello`：起動プロジェクト。**コンソールアプリ（Exe）のまま** Windows Forms のウィンドウを開く。コンソールには対局ログを出す。
- `Othello.Core`：オセロのルール（盤面・合法手・反転・パス・終了判定・待った・CPU）。**画面に関係するコードを書かない。**
- `Othello.Tests`：`Othello.Core` の xUnit テスト。

## コーディング
- .NET 8 / C#。`Nullable` と `ImplicitUsings` は有効のまま。
- ルールの判定は必ず `Othello.Core` に書き、画面（`MainForm` / `BoardControl`）は結果を表示するだけにする。
- 座標は `Position(Row, Col)`。0 始まりで、表示は `a1`〜`h8`（列が a〜h、行が 1〜8）。
- 置けない手は `Game.PlayMove` で例外にせず `MoveResult.Success = false` と理由を返す。
  `Board.Place` は低レベル処理なので、置けない場所では例外を投げる。
- ユーザーに見せる文言は日本語。
- `System.Windows.Forms.Timer` は `System.Threading.Timer` と名前が重なるため、完全修飾名で書く。

## テスト
- ルールを変更したら、正常系・異常系・境界値（角・辺・盤外・満杯・パス・終了）のテストを追加する。
- 盤面は `BoardFactory.FromRows`（`B`=黒、`W`=白、`.`=空き）で作ると読みやすい。

## Git / CI
- 作業はブランチで行い、`main` へのマージは人が実機（Visual Studio で起動）で確認してから行う。
- push / Pull Request で GitHub Actions（windows-latest）がビルド・テスト・実行ファイルの作成を行う。
- 不要な変更（関係ないファイルの整形など）はしない。
