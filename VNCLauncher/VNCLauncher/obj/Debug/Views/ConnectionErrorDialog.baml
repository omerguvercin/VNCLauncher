   M S B A M L   `   `   `  ����F  BVNCLauncher, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null5clr-namespace:VNCLauncher.ViewsVNCLauncher.Views  5   	   -    'VNCLauncher.Views.ConnectionErrorDialog   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   1localclr-namespace:VNCLauncher.Views     5   	     � Title$  Bağlantı Hatası��5   	   $	��220��6!   $	��400��6.    � WindowStartupLocation$ CenterOwner=�5	   	    � WindowStyle$ 
ToolWindow=�5
   	    � 
ResizeMode$ NoResize=�5   	    � ShowInTaskbar$ False��5   	   ���	#FFF0F0F0 � 	Resources 5       �� 
TargetType �� Property �� Value	 �� 
TargetType
 �� CornerRadius   OkButtonStyle �� BasedOn��%?  '��       &	  �    7���� 5   
   " ��6   .�5      ��  � ��$	 100��6&   �� 5       � ��$ 35��6'   �� 5       � ��$ 10q�6'   �� 5       � ��
 �����6+   �� 5       � ��$ 0q�60   �� 5       �@�Cursor$
 Hand��6'   �� 5       � �� 5      �� 5      "	 ��6&   .R�5      �� #��z��6!   $
 4��5   !   #��z��5   !   ._�5      �� $��Center=�6/   $��Center=�5   /   5      5       5"      5#      �� 5$   
   " ��6&   8   .�5%      ��  � �� �#2196F36+   5&      5'      .��5)      �� $
 10��6   
�������$��15q�62   ���#F44336$��2q�6T   ._�5*   
   �  � RowDefinitions 5+      �� 5,      $
��Auto �6    �� 5-      $��* �6    �� 5.      $
��Auto �6    .+�52      �� $��0��6    �� Orientation$ 
Horizontal=�6&   $��25,25,25,10q�6?   $��Center=�6T   .+�53      A� $��36��6   $��36��6%   ���#F44336$��Center=�6@    J� Effect 54      3  -System.Windows.Media.Effects.DropShadowEffect  55         Color$ #B71C1C��6+      
BlurRadius$ 8Q�6;      ShadowDepth$ 2Q�6J   57      �� 58      -   5         ��Name$ txtTitle��58      $��Bağlantı Hatası��6.   $
��Bold"�6E   $��22(�6W   ���#D32F2F$��10,0,0,0q�6z   $��Center=�6�   59      �� 5<      -   5        $ txtErrorMessage��5<   %   $��1��6   $
��Wrap=�6>   $��35,10,35,10q�6R   $��15(�6g   ���#333$��Center=�6�   $��SemiBold"�6�   �� 5?      $��2��6   $ 
Horizontal=�6&   $��Right=�6?   $��
0,15,25,20q�6[   .+�5@      �� -   5        $��Tamam��5@      #��[  6@   $��15(�6g   $	��110��6u   $��38��6�   5A      5B      5C      5D      