   M S B A M L   `   `   `  ���� H  DCommonLibrary, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullfCclr-namespace:Ai.Hong.CommonLibrary.Controls;assembly=CommonLibraryAi.Hong.CommonLibrary.Controls   NirIdentifier-   'NirIdentifier.Calibration.LineSlopeTest   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     SbtnCclr-namespace:Ai.Hong.CommonLibrary.Controls;assembly=CommonLibrary     .��� .+��� -   $��	rootGroup��.���   � RowDefinitions  �� $
��Auto ��� $��1* ��� $
��Auto �.+���  �� Orientation$ 
Horizontal=�.+��� -   $��imageResult��$��0��$4��./NirIdentifier;component/Images/Unknown_32.png��$
��None=�$��5q��� -   $��txtCalibrateName��$��100%线斜率检验�� �� TextAlignment$ Center=�$��Center=�$��10,0,0,0q�$
��12pt(�&   System.Windows.Controls.DataGrid  -   $��dataGrid��$��1��   GridLinesVisibility$
 None=�   AutoGenerateColumns .     CanUserSortColumns .     
IsReadOnly .    SelectionMode$ Single=�   SelectionUnit$ FullRow=�	   Columns	 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn
   Header$
 波数区间��1  +System.Windows.Controls.DataGridBoundColumn   Binding �� *WaveNumRange+ �� StringFormat$ F2��  $
 阈值区间�� �� *	lineLimit+$ F2��  $
 测量最小值�� �� *meaMinValue+$ F2��  $
 测量最大值�� �� *meaMaxValue+$ F2��� $��2��$��20��$��0,10,0,5q� � ColumnDefinitions �� $��1* ��� $
��Auto �.+�� -   $��scanProgress��$��0��
�������
��������� -   $��checkShowSpectrum��$��1��$��显示光谱��$��Bottom=�$��10,0,0,0q�