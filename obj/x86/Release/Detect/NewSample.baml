   M S B A M L   `   `   `  ���� H  DCommonLibrary, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullfCclr-namespace:Ai.Hong.CommonLibrary.Controls;assembly=CommonLibraryAi.Hong.CommonLibrary.Controls  `@clr-namespace:Ai.Hong.CommonLibrary.Chart;assembly=CommonLibraryAi.Hong.CommonLibrary.Chart  fCclr-namespace:Ai.Hong.CommonLibrary.Controls;assembly=CommonLibraryAi.Hong.CommonLibrary.Controls  H DNirIdentifier, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null;"clr-namespace:NirIdentifier.DetectNirIdentifier.Detect ;"clr-namespace:NirIdentifier.CommonNirIdentifier.Common $   NirIdentifier.Detect.NewSample   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     SbtnCclr-namespace:Ai.Hong.CommonLibrary.Controls;assembly=CommonLibrary     Tgraphic@clr-namespace:Ai.Hong.CommonLibrary.Chart;assembly=CommonLibrary     Y	CommonBtnCclr-namespace:Ai.Hong.CommonLibrary.Controls;assembly=CommonLibrary     5result"clr-namespace:NirIdentifier.Detect     3comm"clr-namespace:NirIdentifier.Common     -     � 	Resources  :  4System.Windows.Controls.Primitives.DatePickerTextBox �� 
TargetType �� Triggers P� Property P� Value �� Property �� Value2  ,NirIdentifier.Common.IdentResultImageConvert   resultImageConvert.  (NirIdentifier.Common.MultiAndBoolConvert  
andConvert  	imageCell �� 	Converter��%�  '        '�� S     '�� �     &	  :    &	 ?    &	 D    7   �� "   P�  �@J�IsKeyboardFocusWithin . . ���  � ��
 �P���� " �� P�  �@J�IsKeyboardFocused . . ���  � ��
 �P���� " �� P�  �@J�IsKeyboardFocusWithin . . ���  � ��
 �P��P�  �@J�IsKeyboardFocusWithin .  . ���  � ��
 �����    �� .R��� $��16��$��16������ *identResult+8   .��� $
��10pt(� � ColumnDefinitions �� $��1* ��� $
��Auto ��� $��2* �	 � RowDefinitions	 �� $
��Auto ��� $
��Auto ��� $��1* �.+��� $��0��$��0������ 
 �� Orientation$
 
Horizontal=�.+��� $��模型列表��$	��0,3q��� -   $��checkNotAnalyze��$��2��$��不分析光谱��$��30,3,0,3q�$	��9pt(� � ToolTip$! 只保存光谱，不分析��.��)  #NirIdentifier.Common.ModelInfoPanel  -     Name$ listMethods��)  #NirIdentifier.Common.ModelInfoPanel   ModelSelected$ listMethods_ModelSelected���� $��0��$��1��$��3��$��3�� �� ResizeDirection$ Columns=� �� ResizeBehavior$ PreviousAndNext=� J� Effect 3  -System.Windows.Media.Effects.DropShadowEffect     ShadowDepth$ 1Q�   Opacity$ 50Q�   Color$ 	#FF6C6C6C��   RenderingBias$ Quality=�   
BlurRadius$ 1Q��� $��1��$��0��$��3��$��3��$
 Rows=�$��Stretch=�$ PreviousAndNext=�   $ 1Q�$ 50Q�$ 	#FF6C6C6C��$ Quality=�$ 1Q��� $��0��$��2��
�������$��5,5,5,0q�.��� -   $��gridDrugInfo��	 �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto � �� $
��Auto ��� $��1* ��� $
��Auto ��� $��1* ��� $
��Auto �.+��� $��0��$��0��$��样品编号:��$��Center=�$��Right=��� -   $��txtSampleNumber��$��0��$��1��$��2q����� *sampleNumber+ �� Mode$ TwoWay=��� -   $��listLicenseType��$��0��$��2��$��Center=�$	��100��$��5,0,0,0q� �� SelectedItem �� *licenseType+$ TwoWay=��� -   $��txtLicenseCode��$��0��$��3��$��2q����� *licenseCode+$ TwoWay=��� -   $��btnSeachInfo��$��0��$��4��$��2��$
��Left=�$��5,2,0,2q�$��检索��$��50��$��12(��� -	   $��1��$��0��$��
药品名:��$��Center=�$��Right=��� -
   $��listChemicalName��$��1��$��1��$��2q����� *chemicalName+$ TwoWay=��� $��1��$��2��$��
商品名:��$��Center=�$��Right=��� -   $��txtCommercialName��$��1��$��3��$��2q����� *commercialName+$ TwoWay=��� $��2��$��0��$��剂型:��$��Center=�$��Right=��� -   $��listForm��$��2��$��1��$��2q� �� 
IsEditable .  �� Text �� *form+$ TwoWay=��� $��2��$��2��$��5,0,0,0q�$��规格:��$��Center=�$��Right=��� -   $��txtSpecification��$��2��$��3��$��2q����� *specification+$ TwoWay=��� $��3��$��0��$��生产厂家:��$��Center=�$��Right=��� -   $��listProductUnit��$��3��$��1��$��3,2,1,2q����� *productUnit+$ TwoWay=��� $��3��$��2��$��5,0,0,0q�$��生产日期:��$��Center=�$��Right=�(  "System.Windows.Controls.DatePicker  -     Name$ txtProductTime��$��3��$��3��$��2q�   SelectedDate �� *productTime+$ TwoWay=��� $��4��$��0��$��生产批号:��$��Center=�$��Right=��� -   $��txtBatchNumber��$��4��$��1��$��2q����� *batchNumber+$ TwoWay=��� $��4��$��2��$��
有效期:��$��Center=�$��Right=��� -   $��txtValidMonth��$��4��$��3��$��2q����� *
validMonth+$ TwoWay=��� $��4��$��4��$��个月��$��Center=��� $��5��$��0��$��5,0,0,0q�$��测样方式:��$��Center=�$��Right=��� -   $��listScanType��$��5��$��1��$��2q� �� *
scanType+$ TwoWay=��� $��5��$��2��$��5,0,0,0q�$��鉴别阈值:��$��Center=�$��Right=��� -   $��txtIdentThresold��$��5��$��3��$��2q����� *identThresold+$ TwoWay=��� $��6��$��0��$��5,0,0,0q�$��操作人员:��$��Center=�$��Right=��� -   $��listIdentOperator��$��6��$��1��$��2q� �� *identOperator+$ TwoWay=��� $��6��$��2��$��5,0,0,0q�$��备注:��$��Center=�$��Right=��� -   $��txtMemo��$��6��$��3��$��2��$��2q����� *memo+$ TwoWay=��� $��7��$��0��$��5,0,0,0q�$��检测结果:��$��Center=�$��Right=��� $��7��$��1��
�������
���   �$��1q�$	��5,2q�._��� $��Uniform=�$��64������ *identResult+# [  � $��7��$��2��$��3�� �� $��1* ��� $��1* �.+�/   )Ai.Hong.CommonLibrary.Controls.UserButton  -     Name$ 	btnDetect��$��0��   Text$ 新增样品��$��40��$
��12pt(�$��00,5,0,0q�   	ImageFile$0 */NirIdentifier;component/Images/Add_16.png��{�`�  `� 	Converter# [ .1���   �� Path$  InstrumentConnected�! �� RelativeSource! �� " �� Mode$" FindAncestor=�# �� AncestorType$# WindowL��� $  BackgroundScaned�! �� $" FindAncestor=�$# WindowL�  -   $ btnBackground��$��1��$ 扫描背景��$��40��$
��12pt(�$��20,5,0,0q�$3 -/NirIdentifier;component/Images/detect_24.png��{��� $  InstrumentConnected�! �� $" FindAncestor=�$# WindowL��� $��3��$��0��$��0,5,0,0q����� $
 
Horizontal=�.+��� $��检测结果列表��  -   $ 	btnRemove��$ 移除��$��70��$��20��$��20,0,0,0q�$1 +/NirIdentifier;component/Images/Remove1.png��.��4	   .Ai.Hong.CommonLibrary.Controls.SelectColorList	  -   $��listSampleFiles��4
   .Ai.Hong.CommonLibrary.Chart.MultiSpectrumChart
  -   $ 
 Name$$ graphicChart��$��2��$��2��