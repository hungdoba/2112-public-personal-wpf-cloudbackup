   M S B A M L   `   `   `  ����   AutoBackUpToCloud$    AutoBackUpToCloud.WindowSettup   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     -   $y�Visible=�  � Title$  Backup To Cloud��$	��768��$
��1360�� � 	Resources  �� 
TargetType �� Property �� Value��%-   '�       �� " �.���  � ��$ 16(�.��(� .��� $	��740��$
��1360�� � ColumnDefinitions �� $��1* ��� $��2* ��� $��1* ��� $��2* ��� $��2* � � RowDefinitions �� $��0 ��� $��2* ��� $��1* ��� $��1* ��� $��2* ��� $��2* ��� $��2* ��� $��2* ��� $��2* ��� $��2* �.+��� $��0��$��2��$��2��$��5,5,5,0q�
���   �$	��0.5q��� $��2��$��2��$��2��$��5,5,5,0q�
���   �$	��0.5q��� $��0��$��1��$��Backup Time:��$��Center=��� $��2��$��1��$��Backup Cycle (ms):��$��Center=��� $��0��$��2��$��2��$��Center=�$��Backup Directory��$��Center=��� $��2��$��2��$��2��$��Center=�$��Exclude Directory��$��Center=��� -    ��Name$ txtStartBackupTime��$��1��$��1��$��5q�$��10,5,5,5q�$��Center=��� -   $ txtBackupCycle��$��3��$��1��$��5q�$��10,5,5,5q�$��Center=�&   System.Windows.Controls.DataGrid  -     Name$ gridDirectory��$��3��$��0��$��7��$��2��$��5,0,5,5q�	   AutoGenerateColumns	 .  
   CanUserAddRows
 .     
IsReadOnly .    Columns 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn   Header$ 	Directory��   Width5  /System.Windows.Controls.DataGridLengthConverter$ * 1  +System.Windows.Controls.DataGridBoundColumn   Binding �� *Text+  -   $ gridDirectoryExclude��$��3��$��2��$��7��$��2��$��5,0,5,5q�	 .  
 .   . {�.     $ 	Directory��$ *  �� *Text+�� -    ��Name$ btnShowBackupLog��$��4��$��1��$��Backup History��$��5q�{�. 
��������� -    ��Name$ cbBackupDirectory��$��2��$��4��$��20,0,0,0q�$#��  Setup for Backup Directory?��$��Center=� d� 	IsChecked$
 TrueY��� -   $ cbStartWithSystem��$��3��$��4��$��20,0,0,0q�$�� Start with System��$��Center=� d� IsThreeState .  �� -	   $ btnNewDirectory��$��4��$��4��$��New Directory��$��5q�
��������� -
   $ btnDeleteDirectory��$��4��$��5��$��Delete Directory��$��5q�
��������� -   $ btnInsertToDatabase��$��4��$��6��$��
Save Setup��$��5q�
��������� -   $ btnSetSecurityInformation��$��4��$��7��$��Set Onedrive Information��$��5q�
��������� -   $ btnStart��$��4��$��8��$��Start Watch File��$��5q�
����ح�
��������� -   $ btnStop��$��4��$��9��$��Stop Watch File��$��5q�{�.  
��� E��
��� ���