Gears
=============

Gears Framework(以下Gears)は、ASP.NET WebFormによる開発を高速化するフレームワークです。  
具体的には、「コントロールを配置するだけで画面が作れる」ことを目的に作成されています。  

ASP.NET WebFormは元々こうしたコンセプトに基づいていますが、データベースアクセス部分などそのままでは難しいのが実情です。  
本フレームワークはそれを補完し、 よりこの目的に近い形でのWebアプリケーション作成を実現します。  
また、企業業務システムの構築に際し必要となる機能を多数実装しています。

[Demo Site](http://gearssite.apphb.com/)  
[Demo Site Source](https://github.com/icoxfog417/GearsSite)  

[API Document](http://gearssite.apphb.com/document/Help/Index.aspx)  
[Document](http://gearssite.apphb.com/document/GearsFrameWorkDoc.xls)  

## Gears Framework Main Features

1. **[規約に基づくデータバインド](https://github.com/icoxfog417/Gears/wiki/1.GearsConvention)**  
ASP.NET WebFormでは、コントロールの配置は簡単ですが、それに対するデータロードは煩雑です。  
Gearsでは規約に基づいたIDを設定することで、自動でデータバインドを行う機能を提供します。これによりドロップダウンリストなどのリスト項目の作成は飛躍的に容易になります。  


2. **[関連定義によるデータ連動](https://github.com/icoxfog417/Gears/wiki/2.Relation)**  
あるコントロールに設定された値に応じ他のコントロールの値を変化させる、というのはWebシステムで良くあるパターンです。  
Gearsではコントロール間の関連を定義しておくことで、簡単にこの挙動を実装することができます。  


3. **[コントロールを使用したデータベース処理](https://github.com/icoxfog417/Gears/wiki/3.DatabaseAccess)**  
Gearsではコントロールから値を取り出しSQLを組み立てる必要はありません。  例えばフォームpnlFormに入力された値を保存するなら*GSave(pnlForm)*、検索用のパネルpnlFilterで一覧grvDataを検索するなら*GFilterBy(pnlFilter,grvData)*と書けます。
このように、煩雑な処理なしにコントロールを直接利用し処理が記述できるよう設計されています。


4. **[バリデーション実装](https://github.com/icoxfog417/Gears/wiki/4.Validation)**  
GearsではCssClassの設定のみでコントロールのバリデーションが可能です。  
また、バリデーション内容に応じてスタイルが推定されます。例えば数値型のバリデーションを行うコントロールなら数値型のスタイル(class)が設定される、といった感じです。これにより明示的なスタイル設定は不要になります。  
ビジネスロジックに基づくバリデーションについても実装するための仕組みが用意されています。単にエラーにするだけでなく、警告を出力し確認をしてもらうような処理も簡単に記述できるようになっています。  


5. **[権限制御](https://github.com/icoxfog417/Gears/wiki/5.Authorization)**  
特定の権限があるときのみ表示/非表示にする、また入力可/不可にする、といった制御は業務システムでは多く見られます。  
GearsではASP.NET標準の認証フレームワーク(MembershipProvider/RoleProvider)と連動し、コントロールに属性を設定するだけでこの制御が可能です。  


## Other Features
* **[デバッグ機能](https://github.com/icoxfog417/Gears/wiki/Log)**  
フレームワーク内でどのような処理やSQLが実行されているのかを、簡単にログ出力することができます。  
このログはHTMLで出力し、画面上で確認することも可能です。  

