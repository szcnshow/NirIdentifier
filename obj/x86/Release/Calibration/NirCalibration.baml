   M S B A M L   `   `   `  ���� H  DCommonLibrary, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullfCclr-namespace:Ai.Hong.CommonLibrary.Controls;assembly=CommonLibraryAi.Hong.CommonLibrary.Controls  T:clr-namespace:Ai.Hong.CommonLibrary;assembly=CommonLibraryAi.Hong.CommonLibrary  ^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35tOclr-namespace:System.Windows.Forms.Integration;assembly=WindowsFormsIntegration System.Windows.Forms.Integration m iSystem.Windows.Forms.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35�mclr-namespace:System.Windows.Forms.DataVisualization.Charting;assembly=System.Windows.Forms.DataVisualization/System.Windows.Forms.DataVisualization.Charting W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35U<clr-namespace:System.Windows.Input;assembly=PresentationCoreSystem.Windows.Input H DNirIdentifier, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullE'clr-namespace:NirIdentifier.CalibrationNirIdentifier.Calibration .   (NirIdentifier.Calibration.NirCalibration   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     SbtnCclr-namespace:Ai.Hong.CommonLibrary.Controls;assembly=CommonLibrary     Lchart:clr-namespace:Ai.Hong.CommonLibrary;assembly=CommonLibrary     _wfiOclr-namespace:System.Windows.Forms.Integration;assembly=WindowsFormsIntegration     }CHRmclr-namespace:System.Windows.Forms.DataVisualization.Charting;assembly=System.Windows.Forms.DataVisualization     Ninput<clr-namespace:System.Windows.Input;assembly=PresentationCore     9panel'clr-namespace:NirIdentifier.Calibration     .���   � ColumnDefinitions  �� $
��Auto ��� $
��Auto ��� $��1* �.+�� ���#F0F0F0.+�� $��0��$��0��$	��3,0q�$
��Auto��  �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto �.+�� -   $��laser��$��0��$��0��.+�9� $��0��$��0,4,0,0q� 9� ExpandDirection$ Right=�$��激光波数校正��   ExpanderStyleMain#���   .���  � RowDefinitions �� $
��Auto ��� $
��Auto ��� $��1* ��� $
��Auto �.+��� $��0��$	��0,0q�.+�2  ,NirIdentifier.Calibration.EachCalibratePanel  -     Name$ 
laserPanel��2  ,NirIdentifier.Calibration.EachCalibratePanel   DisplayCheckClicked$ ShowSpc_Laser��$��5q��� $��1�����   �� $
��Auto ��� $
��Auto �.+��� -   $��checkBoxCal��$��0�� d� 	IsChecked$ FalseY��� $��手动校正��$��1��.���� -   $��stackCal�� �� Orientation$ 
Horizontal=�$��	0,20,0,20q�{�.  .+��� $��输入峰位：��$��Center=��� -    ��Name$ peakAuto��$��0,2,0,0q�$��Center=�$��80�� �� IsInputMethodEnabled .  .��	 J� CommandBindings	 )  #System.Windows.Input.CommandBinding  -   
   Command$
 ApplicationCommands.Paste��.��/   )Ai.Hong.CommonLibrary.Controls.UserButton  -      Text$ 校正��$��Center=�   	ImageFile$/ )/NirIdentifier;component/Images/start.png��$
��15,0q�� $��2���� $��3��$��0��$��0, 10q�$ 
Horizontal=�$��Center=�$
��11pt(�.+�  -     Name$ 
btnLWStart��$ 开始校正��$	��120��$��30��$/ )/NirIdentifier;component/Images/start.png��� -	   $��pq��$��0��$��1��.+�9� $��1��$��0,4,0,0q�$��PQ测试��$ Right=�#���   .���  �� $��1* ��� $
��Auto �.+��� -
   $��stack��$��0��.+��� -   $��2��$��9q�$��全选/全不选��
 � Tag$ PQ���� ���� -   $��是否测试��$
 PQPP��.��+  %NirIdentifier.Calibration.PPDeviation  -     Name$ snrPanel��+  %NirIdentifier.Calibration.PPDeviation   DisplayCheckClicked$ ShowSpc_lineNoise��$��5q��� ���� -   $��是否测试��$
 PQAC��.��(  "NirIdentifier.Calibration.Accuracy  -     Name$ accuracyPanel��(  "NirIdentifier.Calibration.Accuracy   DisplayCheckClicked$ ShowSpc_PQaccuracy��$��5q��� ���� -   $��是否测试��$
 PQYA��.��  -   $ 
yaxisPanel��$ ShowSpc_YaxisRep��$��5q��� $��1��$��0, 10q�$ 
Horizontal=�$��Center=�$
��11pt(�.+�  -   $ 
btnPQStart��$ 开始测试��$	��120��$��30��$/ )/NirIdentifier;component/Images/start.png��  -   $ 取消测试��$��20,0,0,0q�$	��120��$��30��$/ )/NirIdentifier;component/Images/start.png��  -   $ btnPQPrintReport��$y�	Collapsed=�$ 打印测试报告��$	��120��$��30��$��20, 0, 0, 0q�$0 */NirIdentifier;component/Images/print1.png��� -   $��oq��$��0��$��2��.+�9� $��2��$��0,4,0,0q�$��OQ测试��$ Right=�#���   .���  �� $��1* ��� $
��Auto �.+��� $��Visible=�$	��0,0q�.���� -   $��oqStack��$��0��.+��� -   $��2��$��9q�$��全选/全不选��$ OQ���� ���� -   $��是否测试��$
 OQRE��.��  -   $ resolutionTest��$ ShowSpc_Resolution��$��5q��� ���� -   $��是否测试��$
 OQPP��.��  -   $ lineNoisePanel��$ ShowSpc_LineNoiseOQ��$��5q��� ���� -   $��是否测试��$
 OQEN��.��  -   $ energyDisPanel��$ ShowSpc_EnergyDis��$��5q��� ���� -   $��是否测试��$
 OQLI��.��-	  'NirIdentifier.Calibration.LineSlopeTest	  -    	 Name$ lineSlopeTest��-
  'NirIdentifier.Calibration.LineSlopeTest 
  DisplayCheckClicked$ ShowSpc_LineSlope��$��5q��� ���� -    $��是否测试��$
 OQTR��.��  -!   $ tranRepPanel��$ ShowSpc_TransRep��$��5q��� ���� -"   $��是否测试��$
 OQAC��.��  -#   $ waveNumAccPanel��$ ShowSpc_WaveNumAcc��$��5q��� ���� -$   $��是否测试��$
 OQWA��.��  -%   $ waveNumRepPanel��$ ShowSpc_WaveNumRep��$��5q��� $��1��$��0, 10q�$ 
Horizontal=�$��Center=�$
��11pt(�.+�  -&   $ 
btnOQStart��$ 开始测试��$	��120��$��30��$/ )/NirIdentifier;component/Images/start.png��  -'   $��20, 0, 0, 0q�$ 取消测试��$	��120��$��30��$/ )/NirIdentifier;component/Images/start.png��  -(   $ btnOQPrintReport��$y�	Collapsed=�$ 打印测试报告��$	��120��$��30��$��20, 0, 0, 0q�$0 */NirIdentifier;component/Images/print1.png��� $��0��$��3��.+�9� $��2��$��0,4,0,0q�$��历史查询��$ Right=�#���   .��� $��0,4,0,0q� �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto �  �� $��1* �.+��� $ 
Horizontal=�$��0��.+��� $��0��$��0��$��起始时间:��$��20,0,0,0q�$��Center=�$��Center=�(  "System.Windows.Controls.DatePicker  -)   $��beginDatePicker��$��0��$��1��$	��120��$��5,0,0,0q�$��Center=�$��Center=��� $ 
Horizontal=�$��1��$��0,10,0,0q�.+��� $��1��$��0��$��结束时间:��$��20,0,0,0q�$��Center=�$��Center=�  -*   $��endDatePicker��$��1��$��1��$	��120��$��5,0,0,0q�$��Center=�$��Center=��� $ 
Horizontal=�$��2��$��0,10,0,0q�.+��� $��查看测试项：��$��20,0,0,0q�$��Center=�$��Center=��� -+   $
��list��$��80��$��5,2,0,0q�.@��� $��PQ测试���� $��OQ测试���� $ 
Horizontal=�$��3��$��0,10,0,0q�.+��� $��具体项：��$��20,0,0,0q��� -,   $��listPqOq��$	��130��$��Stretch=�  --   $ 	btnSearch��$��4��$��0,10,0,0q�$��Center=�$ 立即查询��$	��120��$��30��$0 */NirIdentifier;component/Images/search.png��(   "Ai.Hong.CommonLibrary.GraphicChart  -.     Name$ spectrumChart��$��2��$��2��$y�Visible=����#F0F0F07  1System.Windows.Forms.Integration.WindowsFormsHost  -/     Name$ 	mainFGrid��$
��Auto��$��2��   Child. ;  5System.Windows.Forms.DataVisualization.Charting.Chart  -0   	mainChart��   	BackColorU QSystem.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a#  System.Drawing.ColorConverter$ #F0F0F0 