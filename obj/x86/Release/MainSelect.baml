   M S B A M L   `   `   `  ���� H  DCommonLibrary, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullfCclr-namespace:Ai.Hong.CommonLibrary.Controls;assembly=CommonLibraryAi.Hong.CommonLibrary.Controls   NirIdentifier   NirIdentifier.MainSelect   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     SbtnCclr-namespace:Ai.Hong.CommonLibrary.Controls;assembly=CommonLibrary       � 	Resources   �� 
TargetType   btnStyle �� Property �� Value �� 
TargetType �� CornerRadius �� RadiusX �� RadiusY �� Triggers	 P� Property
 P� Value �� 
TargetName��%!  &	        �� " ��.���  � �� �� " ��.R��� $��
gridButton��$ 10��._�� .+��� $��	blackRect��$ 10��$ 10��$��1��
���   �$��	0,0, 2, 2q��� $��	whiteRect��$ 10��$ 10��$��1��
�������$��2,2,0,0q��� $��
normalRect��$ 10��$ 10��$��1�����	#FFd1d1d1� $��0,0,10,0q�.+��� $��Center=�$��Center=���.  P� 	 �@J�IsMouseOver
 . . ��� $ 	whiteRect�� � y�$ Visible=��� $ 	blackRect�� � y�$ Visible=��� $ 
normalRect�� � y�$ 	Collapsed=��� $ 
gridButton�� � �� �	#FFb2ccffP� 	 �@J�IsMouseOver
 .  . ��� $ 	whiteRect�� � y�$ 	Collapsed=��� $ 	blackRect�� � y�$ 	Collapsed=��� $ 
normalRect�� � y�$ Visible=��� $ 
gridButton�� � �� �	#FFaaabad.���  � RowDefinitions �� $��1* ��� $
��Auto �.+��� $��2��$6��0/NirIdentifier;component/Images/MainSelectBG.jpg��$
��Fill=��� $��Center=�$��Center=�.+��� $��近红外分析系统��
�������$��YouYuan)�$
��Bold"�$
��40pt(�$��Center=�$��Center=� J� Effect 3  -System.Windows.Media.Effects.DropShadowEffect     Color$ Black��   	Direction$ 0Q�   ShadowDepth$ 5Q�   Opacity$ 1Q��� $��Quick Drug ID by NIR��$y�	Collapsed=�
�������$
��Bold"�$
��30pt(�$��Center=�$��Center=�$��Modern No. 20)�   $ Black��$ 0Q�$ 5Q�$ 1Q�� $��1��$��0,0,0,30q� � ColumnDefinitions �� $��1* ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $��1* �.+��� -   $��1��#��[  $	��150��$��60��.����  �� Orientation$ 
Horizontal=�.+��� $7��1/NirIdentifier;component/Images/DrugDetectSel.png��$
��None=��� $��联机检测��$
��14pt(�$��Center=�$��5,0,0,0q��� -   $��2��#��[  $	��150��$��60��$��20,0,0,0q�.���� $ 
Horizontal=�.+��� $4��./NirIdentifier;component/Images/OfflineSel.png��$
��None=��� $��离线分析��$
��14pt(�$��Center=�$��5,0,0,0q��� -   $��3��#��[  $	��150��$��60��$��20,0,0,0q�.���� $ 
Horizontal=�.+��� $6��0/NirIdentifier;component/Images/CalibrateSel.png��$
��None=��� $��仪器验证��$
��14pt(�$��Center=�$��5,0,0,0q��� -   $��4��#��[  $	��150��$��60��$��20,0,0,0q�.���� $ 
Horizontal=�.+��� $7��1/NirIdentifier;component/Images/NormalScanSel.png��$
��None=��� $��常规扫描��$
��14pt(�$��Center=�$��5,0,0,0q��� -   $��5��#��[  $	��150��$��60��$��20,0,0,0q�.���� $ 
Horizontal=�.+��� $8��2/NirIdentifier;component/Images/SystemSetupSel.png��$
��None=��� $��系统设置��$
��14pt(�$��Center=�$��5,0,0,0q�