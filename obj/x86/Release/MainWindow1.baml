   M S B A M L   `   `   `  ���� H  DNirIdentifier, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:NirIdentifierNirIdentifier  ;"clr-namespace:NirIdentifier.CommonNirIdentifier.Common      NirIdentifier.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     .screenclr-namespace:NirIdentifier     2cov"clr-namespace:NirIdentifier.Common     -     � Title$  Quick Drug ID by NIR��$	��500��$	��800�� � WindowState$ 	Maximized=� � Icon$2 ,/NirIdentifier;component/Images/mainIcon.ico�� � 	Resources %   NirIdentifier.Common.TimeTicker 
  ticker �� 
TargetType  btnStyle �� Property �� Value �� 
TargetType � ColumnDefinitions	 �� Orientation
 � RowDefinitions �� Triggers P� Property P� Value �� 
TargetName��%�  &	        &	        �� " 	�.���  � �� �� " 	�.R�� $��
gridButton�� �� $
��Auto ��� $��1* �.+��� $��selMark��$	 
Horizontal=�.+��� $��4��
���  ��$��0��F� 
���  ��"����        �    ��� ���- @KL 	$��Center=�$��0��� $��1��$��0,0,5,0q�
 �� $��1* ��� $
��Auto ��� $
��Auto ��� $��1* ��� $
��Auto ��� $
��Auto �.+��� $��imgSel��$��1��$
��None=����� *
selImage+�� $��imgUnsel��$��1��$
��None=����� *
unselImage+�� $��txtLabel��$��2��$��Center=����	#FF88898b$��0,5,0,0q�$��YouYuan)�$
��11pt(����� *text+�� $��
rectSplite��$��4��$��1�����	#FFaaabad�� $��rectSpliteUnder��$��5��$��1��
�������$��0,0,0,5q� P�  �@d�	IsChecked$
 TrueY�. ��� $ imgSel�� � y�$ Visible=��� $ imgUnsel�� � y�$ 	Collapsed=��� $ selMark�� � y�$ Visible=��� $ txtLabel�� � �� �	#FF137dbd�� $ 
gridButton�� � ��
 ������� $ 
rectSplite�� � y�$ Hidden=��� $ rectSpliteUnder�� � y�$ Hidden=�P�  �@d�	IsChecked$ FalseY�. ��� $ imgSel�� � y�$ 	Collapsed=��� $ imgUnsel�� � y�$ Visible=��� $ selMark�� � y�$ Hidden=��� $ txtLabel�� � �� �	#FF88898b�� $ 
gridButton�� � �� �	#FFDCDDDE�� $ 
rectSplite�� � y�$ Visible=��� $ rectSpliteUnder�� � y�$ Visible=�.��� 
 �� $
��Auto ��� $��1* ��� $
��Auto �.+��� ���	#FF6da1c9$��40��$	 
Horizontal=�.+��� $��24��$3��-/NirIdentifier;component/Images/VspecLogo.png��$��20, 0, 0, 0q��� $��VSpec��$
��16pt(�$��YouYuan)�
�������$��5,0,0,0q�$��Center=� J� Effect 3  -System.Windows.Media.Effects.DropShadowEffect     Color$ Black��   	Direction$	 180Q�   ShadowDepth$ 2Q�   Opacity$ 1Q�� $��1�� �� $
��Auto ��� $��1* �.+��� $��0��$y�Visible=����	#FFDCDDDE$��80��.+�	� -   $��btnHomePage��#��[ $��80��$��90�� d� 	IsChecked$ FalseY�	� -   $��	btnDetect��#��[ $��80��$��90��$ FalseY�$��0,10,0,0q�	� -   $��
btnOffline��#��[ $��80��$��90��$ FalseY�$��0,10,0,0q�	� -   $��btnCalibrate��#��[ $��80��$��90��$ FalseY�$��0,10,0,0q�	� -   $��btnNormalScan��#��[ $��80��$��90��$ FalseY�$��0,10,0,0q�	� -   $��btnSystemSetup��#��[ $��80��$��90��$ FalseY�$��0,10,0,0q�� -    �Name$ GridScreenContainer��$��1��.+�   NirIdentifier.MainSelect  -	     Name$ mainSelelectScreen���� $��2��$��30��$��14(�
���   �$��1q�.@��� $��0,0,20,0q�.���� $	 
Horizontal=�.+��� -
   $��imgState��$
��None=�$��10,0,0,0q�$4��./NirIdentifier;component/Images/Warning_16.png��$y�Visible=��� -   $��	txtStatus��$��正在连接仪器......��$��Center=�$��5,0,0,0q��� $��0,0,20,0q�.���� $	 
Horizontal=�.+��� -   $��imgLaserStatus��$4��./NirIdentifier;component/Images/unknown_16.png��$
��None=�$��10,0,0,0q�$y�	Collapsed=��� $��当前仪器：���� -   $��txtLaser��$�� ��$��Center=�$��5,0,0,0q�$y�Visible=��� $��Right=�.���� $��1��$��Center=�$��Right=�$��5,0,15,0q�����  �� Source# [   �� Path$ Now.Now� �� Mode$ OneWay=� �� StringFormat$ yyyy-MM-dd HH:mm:ss��