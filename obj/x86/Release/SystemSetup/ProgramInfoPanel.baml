   M S B A M L   `   `   `  ���� H  DCommonLibrary, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullfCclr-namespace:Ai.Hong.CommonLibrary.Controls;assembly=CommonLibraryAi.Hong.CommonLibrary.Controls  O Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e0891&clr-namespace:System;assembly=mscorlibSystem H DNirIdentifier, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null;"clr-namespace:NirIdentifier.CommonNirIdentifier.Common 0   *NirIdentifier.SystemSetup.ProgramInfoPanel   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     SbtnCclr-namespace:Ai.Hong.CommonLibrary.Controls;assembly=CommonLibrary     6sys&clr-namespace:System;assembly=mscorlib     3comm"clr-namespace:NirIdentifier.Common     -     � 	Resources  .  (NirIdentifier.Common.SavePathTypeConvert   SaveTypeConvert S� 
MethodName S� 
ObjectType  System.Enum  dataFromEnum S� MethodParameters M� TypeName��%X   &	        &	        S� $ 	GetValues��"   M� $ comm:EnumSavePathType��.��� $��10q�$��14(� � RowDefinitions �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $��1* ��� $
��Auto �.+��� $��0��$��仪器信息��.���� -   $��stackInstrumentInfo�� �� Orientation$ 
Horizontal=�$	��0,5q�.+��� $��使用单位:��$��Center=��� -   $��txtUnitName��$��5,0,0,0q�$��Center=�$	��200������ *
unitName+�� $��仪器编号:��$��20,0,0,0q�$��Center=��� -   $��txtSerialNo��$��Center=�$	��100��$��5,0,0,0q����� *
serialNo+�� -   $��checkCalibration��$��仪器自检��$��20,0,0,0q�$��Center=� d� 	IsChecked �� *needCalibration+�� $��自检频率:��$��10,0,0,0q�$��Center=��� -   $��txtCalibrationTime��$��5,0,0,0q�$��Center=�$��30������ *calibartionTime+�� $��小时��$��Center=��� $��1��$��文件设置��$��0,0,0,0q�.��� -   $��gridSaveInfo��$	��0,5q� � ColumnDefinitions �� $
��Auto ��� $��1* ��� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� $��0��$��光谱保存路径：��$��Center=�$��Right=��� $��1��$��1q�
�����_�$	��0,2q�._��� -   $��txtSavepath��$	��0,1q����� *
savePath+�� -	   $��btnSavePath��$��2��$	��...��$��12(�$��30��$��25���� $��3��$��文件夹命名方式：��$��Center=�$��Right=�$��20,0,0,0q��� -
   $��listSaveType��$��4��$	��100��$
��Left=�$	��2,1q�
����������� 	 �� Source#	 [ 
 �� 	Converter#
 [   �� SelectedItem ��  �� Path$ savePathType� �� Mode$ TwoWay=�#
 [  �� $��2��$��扫描设置��$��0,0,0,0q�.��� -   $��gridScanParameterInfo��$	��0,5q� �� $
��Auto ��� $��1* ��� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� $��0��$��扫描配置文件：��$��Center=�$��Right=��� $��1��$��1q�
�����_�$	��0,2q�._��� -   $��txtSettingFile��$	��0,1q����� *scanSettingFile+�� -   $��btnSettingFilePath��$��2��$	��...��$��12(�$��30��$��20���� $��3��$��扫描次数：��$��Center=�$��Right=�$��20,0,0,0q��� $��4��$	��100��$	��2,1q����� *	scanCount+�� $��3��$��数据字典管理��$��0,10,0,0q�.��� $	��0,5q� �� $
��Auto ��� $��1* ��� $
��Auto � �� $
��0.3* ��� $
��0.7* �.+��� $��0��$��0��$��信息类别��$��Center=��� -   $��listDictKeyword��$��1��$��0��$��2��$	��5,2q��� $��0��$��1��$��常用内容��$��Center=��� -   $��listDictContent��$��1��$��1��$	��5,2q�� $��2��$��1�� �� $��1* ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $��1* �.+�/   )Ai.Hong.CommonLibrary.Controls.UserButton  -     Name$ 
btnDictAdd��$��1��   Text$ 新增��$	��110��$��26��$��5q�   	ImageFile$0 */NirIdentifier;component/Images/New_16.png�� � ToolTip$ 增加常用内容��  -   $ btnDictEdit��$��2��$ 修改��$	��110��$��26��$��5q�$1 +/NirIdentifier;component/Images/Edit_16.png��$ 修改常用内容��  -   $ btnDictDelete��$��3��$ 删除��$	��110��$��26��$��5q�$/ )/NirIdentifier;component/Images/Error.png��$ 删除常用内容��