Gears/Validation/Validator
=============

GearsではCSSClassに特定のスタイルを設定することでバリデーションをかけることができます。  
ここでは、各バリデーションクラスの使用方法について記載します。  

### GByteLength
項目長のチェックを行う。  
`CssClass="gears-GByteLength_Length_3" '項目長が3桁かチェックする`

### GByteLengthBetween
項目長が特定の長さに収まっているかチェックを行う。  
`CssClass="gears-GByteLengthBetween_MinLength_0_Length_18" '項目長が0～18桁かチェックする`

### GCompare
値のチェックを行う。  
`gears-GCompare_OperatorType_>_Expected_0 '値が0より大きいかチェックする`

### GDate
日付のチェックを行う。  
`gears-GDate_Format_yyyyMMdd '日付が指定書式であるかチェックする`

### GMatch
指定した正規表現のパターンにマッチするかチェックする。
`gears-GMatch_Pattern_^/.+\.aspx '/で始まり.aspxで終わるかチェックする`

### GNumber
整数(正の値であるか)かどうかチェックを行う。  
`gears-GNumber `

### GNumeric
数値かどうかチェックを行う(符号、小数点なども考慮可)。  
`gears-GNumeric `

### GPeriodPositionOk
整数部と小数点以下桁数のチェックを行う。  
`gears-GPeriodPositionOk_BeforeP_4_AfterP_2 '整数部4桁、小数部2桁以内かチェックを行う`

### GRequired
入力があるかチェックを行う。  
`gears-GRequired`

### GStartWith
特定文字列で始まることをチェックする。  
`gears-GStartWith_Prefix_B 'Bで始まるかチェックする `

