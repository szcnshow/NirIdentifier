   M S B A M L   `   `   `  ���� H  DCommonLibrary, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullfCclr-namespace:Ai.Hong.CommonLibrary.Controls;assembly=CommonLibraryAi.Hong.CommonLibrary.Controls   NirIdentifier+   %NirIdentifier.Calibration.PPDeviation   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     SbtnCclr-namespace:Ai.Hong.CommonLibrary.Controls;assembly=CommonLibrary     .��� .+��� -   $��	rootGroup��.���   � RowDefinitions  �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto �.+���  �� Orientation$ 
Horizontal=�.+��� -   $��imageResult��$��0��$4��./NirIdentifier;component/Images/Unknown_32.png��$
��None=�$��5q��� -   $��txtCalibrateName��$!��100%线噪声 100%线偏差��$��Center=�$��10,0,0,0q�$
��12pt(��� -   $��ppStack��$��1��$ 
Horizontal=�
��� ���$
��11pt(�$��0,5,0,0q�.+��� $��噪声阈值：���� $��1q�
���   �._��� -   $��txtLineNioseThresold��$��60��$
��0.1%�� �� TextAlignment$ Center=��� $��测量值：��$��10,0,0,0q��� $��1q�
���   �._��� -   $��txtRealLineNoiseValue��$��60���� -   $��devStack��$��2��$ 
Horizontal=�
��� ���$
��11pt(�$��0,5,0,0q�.+��� $��偏差阈值：���� $��1q�
���   �._��� -   $��txtDevThresold��$
��0.5%��$ Center=�$��60���� $��测量值：��$��10,0,0,0q��� $��1q�
���   �._��� -	   $��txtRealDevValue��$��60���� $��3��$ 
Horizontal=�
��� ���$
��11pt(�$��0,5,0,0q�.+��� $��干涉阈值：���� $��1q�
���   �._��� -
   $��txtIpa��$
��0.5%��$ Center=�$��60���� $��测量值：��$��10,0,0,0q��� $��1q�
���   �._��� -   $��
txtRealIpa��$��60��$��0,-1,0,1q��� $��4��$ 
Horizontal=�
��� ���$
��11pt(�$��0,5,0,0q�.+��� $��能量阈值：���� $��1q�
���   �._��� -   $��txtEng��$
��0.5%��$ Center=�$��60���� $��测量值：��$��10,0,0,0q��� $��1q�
���   �._��� -   $��
txtRealEng��$��60��� $��5��$��20��$��0,10,0,0q� � ColumnDefinitions �� $��1* ��� $
��Auto �.+�� -   $��scanProgress��$��0��
�������
��������� -   $��checkShowSpectrum��$��1��$y�	Collapsed=�$��显示光谱��$��Bottom=�$��10,0,0,0q��� $ 
Horizontal=�$��Center=�$��6��$��0,5,0,5q�.+��� -   $	��ifg��$��Bottom=�
 � Tag$	 ifg��$��15,0,0,0q��� -   $	��sbm��$��Bottom=�$	 sbm��$��20,0,0,0q��� -   $��tr��$��Bottom=�$ tr��$��20,0,0,0q�