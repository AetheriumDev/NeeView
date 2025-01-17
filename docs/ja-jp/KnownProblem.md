## :pushpin: 最新版での不具合

こちらの [課題リスト](https://bitbucket.org/neelabo/neeview/issues?kind=bug&status=resolved&status=open&status=new&status=wontfix) から確認できます。

----

## :pushpin: 確認されている問題

以下の問題は現状では仕様となります


### :small_blue_diamond: "CopyTrans HEIC for Windows" を使用するとクラッシュする

HEIC画像を表示可能にする"CopyTrans HEIC for Windows"というソフトがありますが、NeeViewでは動作が不安定になり、NeeView自体が起動できないこともあります。
このため、"CopyTrans HEIC for Windows"はサポート外といたします。

Windows10でHEIC画像を表示させるためには、ストアから[HEIF 画像拡張機能](https://www.microsoft.com/ja-jp/p/heif-%E7%94%BB%E5%83%8F%E6%8B%A1%E5%BC%B5%E6%A9%9F%E8%83%BD/9pmmsr1cgpwg#activetab=pivot:overviewtab)をインストールしてお試しください。

### :small_blue_diamond: Susieプラグインの問題

Susieプラグインとの相性問題があります。使用するプラグインを限定して使用されることをお薦めします。

* 機能しないプラグインがある
* 圧縮ファイル中の画像を開こうとすると落ちる
* 1回目は読み込めるが2回目に読み込もうとすると強制終了する（同時に使用するプラグインとの組み合わせ？）