   M S B A M L   `   `   `  ���� H  DCommonLibrary, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullT:clr-namespace:Ai.Hong.CommonLibrary;assembly=CommonLibraryAi.Hong.CommonLibrary  H DNirIdentifier, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null;"clr-namespace:NirIdentifier.CommonNirIdentifier.Common ;"clr-namespace:NirIdentifier.DetectNirIdentifier.Detect *   $NirIdentifier.FileFormat.FileOpenDlg   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jsrc:clr-namespace:Ai.Hong.CommonLibrary;assembly=CommonLibrary     2cov"clr-namespace:NirIdentifier.Common     5result"clr-namespace:NirIdentifier.Detect     -     � Title$  FileOpenDlg��$	��300��$	��220��
������� � 	Resources '  !NirIdentifier.Common.ImageConvert   
ImgConvert2  ,NirIdentifier.Common.IdentResultImageConvert  resultImageConvert��%   &	        &	          .��� -   $��gridDrugInfo��$��14(�$��5,20,5,5q� � RowDefinitions �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto � � ColumnDefinitions �� $
��Auto ��� $��1* �.+��� $��0��$��0��$��批号:��$��2q�$��Right=��� $��0��$��1��$��2q����� *licenseCode+�� $��1��$��0��$��药名:��$��2q�$��Right=��� $��1��$��1��$��2q����� *chemicalName+�� $��2��$��0��$��厂家:��$��2q�$��Right=��� $��2��$��1��$��2q����� *productUnit+�� $��3��$��规格:��$��0,2,2,0q�$��Right=�$��18��$	��Top=��� $��3��$��1��$��2q����� *specification+�� $��4��$��模型:��$��0,2,2,0q�$��Right=�$��18��$	��Top=��� $��4��$��1��$��2q����� *
identModel+�� $��5��$��时间:��$��0,2,2,0q�$��Right=�$��18��$	��Top=��� $��5��$��1��$��2q����� *	identTime+�� $��6��$��0��$��人员:��$��2q�$��Right=��� $��6��$��1��$��2q����� *identOperator+�� $��7��$��0��$��单位:��$��2q�$��Right=��� $��7��$��1��$��2q����� *	identUnit+�� $��8��$��0��$��结果:��$��2q�$��Right=��� $��8��$��1��$��16��$��16��$
��Left=�$��2q����� *identResult+ �� 	Converter# [ �� $��9��$��0��$��备注:��$��2q�$��Right=��� $��9��$��1��$��2q����� *memo+