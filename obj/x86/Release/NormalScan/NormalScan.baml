   M S B A M L   `   `   `  ���� H  DCommonLibrary, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullfCclr-namespace:Ai.Hong.CommonLibrary.Controls;assembly=CommonLibraryAi.Hong.CommonLibrary.Controls  `@clr-namespace:Ai.Hong.CommonLibrary.Chart;assembly=CommonLibraryAi.Hong.CommonLibrary.Chart  H DNirIdentifier, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null;"clr-namespace:NirIdentifier.CommonNirIdentifier.Common )   #NirIdentifier.NormalScan.NormalScan   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     SbtnCclr-namespace:Ai.Hong.CommonLibrary.Controls;assembly=CommonLibrary     Ogh@clr-namespace:Ai.Hong.CommonLibrary.Chart;assembly=CommonLibrary     3comm"clr-namespace:NirIdentifier.Common       � 	Resources  .  (NirIdentifier.Common.MultiAndBoolConvert   
andConvert��%   &	          .��� $��14(�
������� � ColumnDefinitions �� $
��Auto ��� $
��Auto ��� $��1* �.+�� $��0��
�������$	��5,0q� � RowDefinitions �� $
��Auto ��� $��1* ��� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� $��
10,10,10,0q�._��� $��扫描文件列表��$��Center=�$	��0,3q�4   .Ai.Hong.CommonLibrary.Controls.SelectColorList  -     Name$ 	listFiles��$��1��$	��9pt(��� $��2�� �� Orientation$ 
Horizontal=�$��Center=�$��
0, 5, 0, 0q�.+�/   )Ai.Hong.CommonLibrary.Controls.UserButton  -   $��btnLoad��   Text$ 载入��   	ImageFile$8 2/NirIdentifier;component/Images/LoadMethodIcon.gif��$��80��$��25��  -   $��	btnRemove��$ 卸载��$1 +/NirIdentifier;component/Images/Remove1.png��$��20,0,0,0q�$��80��$��25���� $��3��$��参数设置��$��10q�.��� -   $��gridPara��$��
0, 5, 0, 0q� �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto � �� $
��Auto ��� $��1* ��� $
��Auto ��� $��1* �.+��� $��0��$��0��$��配置文件：��$��Center=��� $��0��$��1��$��2��$��71��$��1q�
����  �$��Center=�._��� -   $��iniFile��$��1,0,-1,0q� �� 
IsReadOnly . �� -   $��btnSettingBrowse��$��0��$��3��$	��...��$��40��$��20��$	��5,2q��� $��1��$��0��$��扫描次数：��$��Center=��� -   $��1��$��1��$��2��$	��0,2q� �� IsInputMethodEnabled .  ���� *	scanCount+	 �� Mode$	 TwoWay=�.��
 J� CommandBindings
 )  #System.Windows.Input.CommandBinding  -      Command$ ApplicationCommands.Paste��.���� $��2��$��0��$��分辨率：��$��Center=�$��Right=��� -	   $��2��$��1��$��2��$	��0,2q� .  ���� *
resolution+$	 TwoWay=�.��
   -
   $ ApplicationCommands.Paste��.���� $��3��$��0��$��扫描区间：��$��Center=�$��Right=��� -   $��3��$��1��$	��0,2q� .  ���� *firstX+$	 TwoWay=�.��
   -   $ ApplicationCommands.Paste��.���� $��3��$��2��$	�� - ��$��Center=�$��Center=��� -   $��3��$��3��$	��0,2q� .  ���� *lastX+$	 TwoWay=�.��
   -   $ ApplicationCommands.Paste��.���� $��4��$��0,0,0,20q�$ 
Horizontal=�$��Center=�.+�  -   $��btnScanSample��$	��100��$��35��$ 扫描样品��$/ )/NirIdentifier;component/Images/start.png��{�`�  `� 	Converter# [  .1���  �� Path$ InstrumentConnected� �� RelativeSource ��  �� Mode$ FindAncestor=� �� AncestorType$ WindowL��� $ BackgroundScaned� �� $ FindAncestor=�$ WindowL�  -   $��btnScanBackground��$	��100��$��35��$ 扫描背景��$3 -/NirIdentifier;component/Images/Verify_24.png��$��30,0,0,0q�{��� $ InstrumentConnected� �� $ FindAncestor=�$ WindowL��� $��1��$��3�� �� ResizeBehavior$ PreviousAndNext=�$��Stretch=� J� Effect 3  -System.Windows.Media.Effects.DropShadowEffect     ShadowDepth$ 1Q�   Opacity$ 50Q�   Color$ 	#FF6C6C6C��   RenderingBias$ Quality=�   
BlurRadius$ 1Q�4   .Ai.Hong.CommonLibrary.Chart.MultiSpectrumChart  -     Name$ graphicChart��$��2��