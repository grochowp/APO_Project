   M S B A M L   `   `   `  ���� F  BAPO_Projekt, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null;"clr-namespace:APO_Projekt.FeaturesAPO_Projekt.Features  ,    &APO_Projekt.Features.OperationSelector   W SPresentationCore, Version=7.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=7.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=7.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     4local"clr-namespace:APO_Projekt.Features       � Title$  OperationSelector��$	��300��$	��300��.���  � ColumnDefinitions �� $	��17* ��� $	��50* �.+��� $��	Operacja:��$
��Left=�$��	29,25,0,0q�$	��Top=�$��2���� -   $��cmbOperation��$��Center=�$��0,29,0,0q�$	��Top=�$	��120��$��1��.@���  �� 
IsSelected$
 True��.��Add�� .��	Substract�� .��Blend�� .��AND�� .��OR�� .��NOT�� .��XOR�� $��Blend:��$
��Left=�$��	38,71,0,0q�$	��Top=�$��2���� -    ��Name$ 
blendInput��$��Center=�$��0,75,0,0q� �� TextWrapping$
 Wrap=�$	��Top=�$	��120��$��1���� $��Pierwsze zdjęcie:��$
��Left=�$��
10,149,0,0q�$	��Top=�$��2���� -   $��firstImg_cmb��$��Center=�$��	0,151,0,0q�$	��Top=�$	��120��$��1���� $��Drugie zdjęcie:��$
��Left=�$��
16,188,0,0q�$	��Top=�$��2���� -   $��secondImg_cmb��$��Center=�$��	0,190,0,0q�$	��Top=�$	��120��$��1���� -    ��Name$ ButtonMedianFiltration��$
��Next��$��Center=�$	��Top=�$��46��$��	0,241,0,0q�$��1��