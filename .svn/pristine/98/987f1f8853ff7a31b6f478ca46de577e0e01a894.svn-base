<%@ Page Language="VB" MasterPageFile="pppMaster.master" AutoEventWireup="false" CodeFile="default.aspx.vb" Inherits="_400_kakaku_menu" %>

<asp:Content id="clientHead" ContentPlaceHolderID="pppHead" Runat="Server" ClientIDMode=Static>
    <title>GearsFramework</title>
    <script language="javascript">
        $(function () {
            setPPPMenuAction();
        })
        
    </script>
</asp:Content>

<asp:Content id="clientOptionMenu" ContentPlaceHolderID="pppOptionMenu" Runat="Server" ClientIDMode=Static>
</asp:Content>

<asp:Content id="clientCenter" ContentPlaceHolderID="pppContent" Runat="Server" ClientIDMode=Static>
    <div align="center">
    <br/>
    <span class="ppp-caption" style="font-size:large;">GearsFrameworkについて</span>
    <!-- ▼各機能へのリンク -->
    <div id="ppp-menu" class="ppp-menu" style="height:550px">
		<div id="ppp-menu-list">
            <ul class="ppp-menu-list">
				<li class="ppp-menu-item">
					<a href="GearsSampleControl.aspx" target="_detail" class="ppp-link-item">GFWでの<br/>データソースと規約</a>
				</li>
				<li class="ppp-menu-item">
					<a href="GearsSampleRelation.aspx" target="_detail" class="ppp-link-item">GFWでの<br/>関連とデータ連動</a>
				</li>
				<li class="ppp-menu-item">
					<a href="GearsSampleValidation.aspx" target="_detail" class="ppp-link-item">GFWの<br/>バリデーション機能</a>
				</li>
				<li class="ppp-menu-item">
					<a href="GearsSampleAuthorization.aspx" target="_detail" class="ppp-link-item">GFWの<br/>権限制御機能</a>
				</li>
				<li class="ppp-menu-item">
					<a href="GearsSampleLog.aspx" target="_detail" class="ppp-link-item">GFWの<br/>ログ/例外処理機能</a>
				</li>
				<li class="ppp-menu-item">
					<a href="GearsSampleStyle.aspx" target="_detail" class="ppp-link-item">GFWのスタイル</a>
				</li>
				<li class="ppp-menu-item">
					<a href="GearsSample.aspx" target="_detail" class="ppp-link-item">GFW総集編</a>
				</li>

			</ul>
		</div>
		<div id="ppp-menu-content" >
            <div class="ppp-menu-content-item default" >
                <span class="ppp-caption">はじめに</span><br/><br/>
                GearsFramework(以下<i>GFW</i>)は、ASP.NET WebFormによる開発を高速化するフレームワークです。<br/>
                具体的には、<b>「コントロールを配置するだけで画面が作れる」</b>ことを目的に作成されています。<br/>
                ASP.NET WebFormは元々こうしたコンセプトに基づいていますが、標準だけでは難しいのが実情です。本フレームワークはそれを補完し、
                よりこの目的に近い形でのWebアプリケーション作成を実現します。<br/>
                本フレームワークが補完する機能は以下点です。
                <ul>
                    <li><u>規約に基づくコントロールへのデータロード</u><br/>
                        ASP.NET WebFormでは、コントロールの配置は簡単ですがそこに対するデータロードは煩雑です。<br/>
                        <i>GFW</i>ではデータを抽出する処理をクラスへ分離します。これによりデータ抽出処理の再利用性を高め、
                        また規約に基づいた名称をコントロール・クラス間に設定することでデータロードを簡略化します。
                    </li>    
                    <li><u>コントロール間の関連定義による、データの連動</u><br/>
                        あるコントロールの値を他のコントロールへ反映させるという良くあるパターンを、簡単に実現できます。<br/>
                        <i>GFW</i>では上述の通りコントロールとデータを抽出するクラスを規約により関連付けているため、コントロール間の関連を定義することで
                        データの連動が実現できます。
                    </li>
                        
                    <li><u>コントロールのスタイル定義によるバリデーション</u><br/>
                        あるコントロールを数値書式で表示するならば、数値に関するバリデーションが実行されるべきです。<br/>
                        <i>GFW</i>ではこのような思想で、スタイル(CSS)定義によるバリデーションを可能にしています。
                    </li>

                    <li><u>コントロールのに対する権限制約</u><br/>
                        このロールがあるときしかボタンを押せないようにしたい、というのは良くあることです。<br/>
                        <i>GFW</i>では、コントロールへの属性設定により、簡単にこの機能を実現することが出来ます。
                    </li>

                    <li><u>ログ参照機能</u><br>
                        <i>GFW</i>ではログ機能を使用することで、画面の項目にどのような入力値が与えられ、それを元にどのようなSQLが実行されたか、
                        その結果はどうだったかがトレースできます。
                    </li>
                </ul>
                <br/>
                <a href="./document/GearsFrameWorkDoc.xls" class="ppp-link-item">仕様書ダウンロードはこちら</a>

            </div>
            <div class="ppp-menu-content-item">
                <span class="ppp-caption">GFWでのデータソースと規約</span><br/><br/>
                <i>GFW</i>におけるコントロールへのデータロードに当たっては、以下2点がポイントとなります。<br/>
                <ul>
                    <li>１．データの抽出処理はクラス(以下、<i>データソースクラス</i>)へ切り出す。</li>
                    <li>２．コントロールと<i>データソースクラス</i>は、名称の規約により自動的に関連付けする。</li>
                </ul>
                １の目的は、データ抽出処理の再利用性を高めることにあります。別々のページで同じCategoryというドロップダウンリストを作成するのに、
                データセットのためのコードを何度もコピー＆ペーストするのは生産的ではないためです。<br/>
                <br/>
                ２の目的は、コーディングの高速化にあります。ddlCategoryというドロップダウンリストにCategoryというデータをロードしたいのは、
                正直見れば分かることであり、これをいちいちコードで書く必要はないためです。<br/>
                <br/>
                <i>GFW</i>でコントロールにデータをセットするためにやらなければならないことは、<i>データソースクラス</i>の作成と規約に基づく名称設定のみになります。
            </div>
            <div class="ppp-menu-content-item">
                <span class="ppp-caption">GFWでの関連とデータ連動</span><br/><br/>
                あるコントロールの値をあるコントロールに連動させたいというのは良くあることです。例えば、Categoryの値を設定したらそれでItemのリストを絞りたい、などです。<br/>
                <br/>
                <i>GFW</i>ではこの処理を２行で実現することが出来ます。<br/>
                <br/>
                標準ではそう簡単にはいきませんし、操作的には簡単であったも大量のコードが自動生成されたりするのは良くあることです。<br/>
                <i>GFW</i>でこれが実現できるのは、<i>データソースクラス</i>と規約を導入しているためです。
                <br/>
            </div>
            <div class="ppp-menu-content-item">
                <span class="ppp-caption">GFWのバリデーション機能</span><br/><br/>
                 コントロールにはデータ型(文字列型・数値型etc・・・)があり、データ型にはそれに応じた表示・バリデーション方式があります。<br/>
                 例えば、数値型であれば右寄せで表示・0～9の値で構成されるかチェック、などです。<br/>
                 <br/>
                 <i>GFW</i>では表示のスタイルとバリデーションは関連するものとして設計しており、スタイル(CSS)定義によるバリデーションを可能にしています。
                 <br/><br/>
                 また、複数の項目の整合性をチェックするようなビジネスロジック検証についても、簡単に実装できるよう設計されています<br/><br/>
            </div>
            <div class="ppp-menu-content-item">
                <span class="ppp-caption">GFWの権限制御機能</span><br/><br/>
                権限による制約は、ページへのアクセス可否だけでなくページ内の操作にも影響することが良くあります。<br/>
                例えば、このロールを持っている人は編集できるが、それ以外の人は出来ないようにしたい、などの要件です。<br/>
                <br/>
                <i>GFW</i>では権限による制約をコントロールの属性に追加することで、コントロールの有効・無効(具体的にはEnable)を切り替えることが出来ます。
                 <br/>
                 <br/>
            </div>
            <div class="ppp-menu-content-item">
                <span class="ppp-caption">GFWのログ/例外処理機能</span><br/><br/>
                フレームワークを使用していて困るのが、エラーが発生した際に内部でどのような処理が行われたのか良く分からないことです。
                開発を行う側にとって、フレームワーク依存のエラーほど面倒なものはありません。<br/>
                <br/>
                <i>GFW</i>ではこうした面倒を避けるために、フレームワーク内での処理をログとして出力する機能があります。<br/>
                具体的には、画面上にどのような項目が入力され、それをどのように解釈し、SQLを実行したのかが詳細にトレースできます。<br/>
                フレームワーク内だけでなく自分の書いたコード内でもログ出力ができるため、デバッグツールとしても使用できます。
                 <br/>
                 <br/>
            </div>
            <div class="ppp-menu-content-item">
                <span class="ppp-caption">GFWのスタイル</span><br/><br/>
                <i>GFW</i>では、なるべくコントロールの配置だけで開発が出来るよう便利なユーザーコントロールUnitItemを用意しています。<br/>
                <br/>
                UnitItemを使用することで、以下のようなスタイルがコントロールの属性設定だけで可能になります。<br/>
                <ul>
                    <li>ラベルとコントロールをセットで配置</li>
                    <li>検索ヘルプなどの、ボタンつきコントロールの配置</li>
                    <li>整ったコントロールの配置</li>
                </ul>

                 <br/>
                 <br/>
            </div>
            <div class="ppp-menu-content-item">
                <span class="ppp-caption">GFW総集編</span><br/><br/>
                全ての処理を使用したサンプル画面です
            </div>
        </div>
	</div>
    </div>
    <asp:HiddenField ID="hdLoginUser" runat="server" ClientIDMode=Static />
</asp:Content>
