   M S B A M L   `   `   `  ���� H  DCommonLibrary, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullfCclr-namespace:Ai.Hong.CommonLibrary.Controls;assembly=CommonLibraryAi.Hong.CommonLibrary.Controls   NirIdentifier-   'NirIdentifier.SystemSetup.DrugInfoPanel   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     SbtnCclr-namespace:Ai.Hong.CommonLibrary.Controls;assembly=CommonLibrary     -   .���   � RowDefinitions  �� $��1* ��� $
��Auto ��� $
��Auto ��� $
��Auto �.+�&   System.Windows.Controls.DataGrid  -     Name$ viewDrugInfo��$��0��   AutoGenerateColumns .  $��14(�   CanUserAddRows .    Columns 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn   Header$ 药监码/条码��   Width5  /System.Windows.Controls.DataGridLengthConverter$
 0.5* 1  +System.Windows.Controls.DataGridBoundColumn   Binding ��  �� Path$ barCode�	 �� Mode$	 TwoWay=�
 �� UpdateSourceTrigger$
 PropertyChanged=�  $ 	药品名��$ 1*  �� $ chemicalName�$	 TwoWay=�$
 PropertyChanged=�  $ 生产厂家��$ 1*  �� $ productUnit�$	 TwoWay=�$
 PropertyChanged=�  $ 剂型��$
 0.5*  �� $
 form�$	 TwoWay=�$
 PropertyChanged=�  $ 标示体积(ml)��$
 0.5*  �� $ 
totalValue�$	 TwoWay=�$
 PropertyChanged=�  $ 主成分(mg)��$
 0.5*  �� $ APIValue�$	 TwoWay=�$
 PropertyChanged=�  $ 包装��$
 0.5*  �� $ package�$	 TwoWay=�$
 PropertyChanged=�  $ 批准文号��$
 0.5*  �� $ licenseCode�$	 TwoWay=�$
 PropertyChanged=��� $��1�� �� Orientation$ 
Horizontal=�$��Center=�.+�/   )Ai.Hong.CommonLibrary.Controls.UserButton  -     Name$ 	btnDelete��   Text$ 删除数据��$��20,10q�$	��120��$��30��   	ImageFile$/ )/NirIdentifier;component/Images/Error.png��  -   $ btnImportData��$ 导入数据��$��20,10q�$	��120��$��30��$0 */NirIdentifier;component/Images/import.png��  -   $ btnExportData��$ 导出数据��$��20,10q�$	��120��$��30��$0 */NirIdentifier;component/Images/export.png��