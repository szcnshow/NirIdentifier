   M S B A M L   `   `   `  ���� H  DCommonLibrary, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullfCclr-namespace:Ai.Hong.CommonLibrary.Controls;assembly=CommonLibraryAi.Hong.CommonLibrary.Controls  `@clr-namespace:Ai.Hong.CommonLibrary.Chart;assembly=CommonLibraryAi.Hong.CommonLibrary.Chart  H DNirIdentifier, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null;"clr-namespace:NirIdentifier.CommonNirIdentifier.Common ;"clr-namespace:NirIdentifier.DetectNirIdentifier.Detect (   "NirIdentifier.Offline.SampleResult   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     SbtnCclr-namespace:Ai.Hong.CommonLibrary.Controls;assembly=CommonLibrary     Tgraphic@clr-namespace:Ai.Hong.CommonLibrary.Chart;assembly=CommonLibrary     2cov"clr-namespace:NirIdentifier.Common     5result"clr-namespace:NirIdentifier.Detect       � 	Resources  '  !NirIdentifier.Common.ImageConvert   
ImgConvert,  &NirIdentifier.Common.DateTimeConverter  timeConvert2  ,NirIdentifier.Common.IdentResultImageConvert  resultImageConvert  	imageCell �� 	Converter��%x   &	        &	      &	 
     &	      7        �� .R��� $��16��$��16������ *identResult+8   .���  � RowDefinitions �� $��1* ��� $
��Auto ��� $
��Auto ��� $
��Auto �.+�4   .Ai.Hong.CommonLibrary.Controls.SelectColorList  -     Name$ 	listFiles���� $��1��$��3�� �� ResizeDirection$
 Rows=� �� ResizeBehavior$ PreviousAndNext=�$��Stretch=�� $��2��$��14(� � ColumnDefinitions �� $��1* ��� $
��Auto ��� $��1* �.+�� -   $��gridDrugInfo��$��0�� �� $
��Auto ��� $
��Auto �.+��� $��0��$��药品信息��.���  �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto � �� $
��Auto ��� $��1* ��� $
��Auto ��� $��1* �.+��� $��0��$��0��$��样品编号:��$��Center=�$��Right=��� $��0��$��1��$��2q����� *sampleNumber+�� $��0��$��2��$��
注册码:��$��Center=�$��Right=��� $��0��$��3��$��2q����� *licenseCode+�� $��1��$��0��$��
药品名:��$��Center=�$��Right=��� $��1��$��1��$��2q����� *chemicalName+�� $��1��$��2��$��
商品名:��$��Center=�$��Right=��� $��1��$��3��$��2q����� *commercialName+�� $��2��$��0��$��剂型:��$��Center=�$��Right=��� $��2��$��1��$��2q����� *form+�� $��2��$��2��$��规格:��$��Center=�$��Right=��� $��2��$��3��$��2q����� *specification+�� $��3��$��0��$��生产厂家:��$��Center=�$��Right=��� $��3��$��1��$��2q����� *productUnit+�� $��3��$��2��$��生产日期:��$��Center=�$��Right=��� $��3��$��3��$��2q����� *productTime+ �� StringFormat$ 
yyyy-MM-dd���� $��4��$��0��$��生产批号:��$��Center=�$��Right=��� $��4��$��1��$��2q����� *batchNumber+�� $��4��$��2��$��有效期(月):��$��Center=�$��Right=��� $��4��$��3��$��2q����� *
validMonth+�� $��1��$��检测信息��.���  �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto � �� $
��Auto ��� $��1* ��� $
��Auto ��� $��1* �.+��� $��0��$��0��$��检测结果:��$��Center=�$��Right=��� $��0��$��1��$��2q����� *identResult+�� $��0��$��2��$��
结果值:��$��Center=�$��Right=��� $��0��$��3��$��2q����� *
identValue+�� $��1��$��0��$��分析模型:��$��Center=�$��Right=��� $��1��$��1��$��2q����� *
identModel+�� $��1��$��2��$��设定阈值:��$��Center=�$��Right=��� $��1��$��3��$��2q����� *identThresold+�� $��2��$��0��$��检测时间:��$��Center=�$��Right=��� $��2��$��1��$��2q����� *	identTime+$ yyyy-MM-dd HH:mm:ss���� $��2��$��2��$��检测单位:��$��Center=�$��Right=��� $��2��$��3��$��2q����� *	identUnit+�� $��3��$��0��$��
检测员:��$��Center=�$��Right=��� $��3��$��1��$��2q����� *identOperator+�� $��3��$��2��$��仪器编号:��$��Center=�$��Right=��� $��3��$��3��$��2q����� *identInstrumentID+�� $��4��$��0��$��备注:��$��Center=�$��Right=��� -   $��txtMemo��$��4��$��1��$��2q�$��3������ *memo+�� $��1��$��3��$ Columns=�$ PreviousAndNext=�$	��2,0q�4   .Ai.Hong.CommonLibrary.Chart.MultiSpectrumChart  -     Name$ graphicChart��$��2���� $��3��
�������._��� 	 �� Orientation$	 
Horizontal=�$��Center=�.+�/   )Ai.Hong.CommonLibrary.Controls.UserButton  -   
  Name$
 btnLoad��$��1��   Text$ 加载文件��$	��120��$��30��   	ImageFile$8 2/NirIdentifier;component/Images/LoadMethodIcon.gif��$��5q�  -   $
 	btnUnload��$��2��$ 卸载文件��$	��120��$��30��$1 +/NirIdentifier;component/Images/Remove1.png��$��10,5,5,5q�  -   $
 btnReIdentify��$��3��$ 重新鉴别��$	��120��$��30��$4 ./NirIdentifier;component/Images/culculator.png��$��10,5,5,5q�  -   $
 btnPrint��$��4��$ 打印输出��$	��120��$��30��$0 */NirIdentifier;component/Images/print1.png��$��10,5,5,5q�