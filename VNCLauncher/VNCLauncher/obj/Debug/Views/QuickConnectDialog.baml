   M S B A M L   `   `   `  ����  VNCLauncher*    $VNCLauncher.Views.QuickConnectDialog   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  Hızlı Bağlantı��5   	    � SizeToContent$ WidthAndHeight=�5   	    � WindowStartupLocation$ CenterOwner=�6(    � 
ResizeMode$ NoResize=�6L   ���#F0F0F0 � ShowInTaskbar$ False��5       � WindowStyle$ 
ToolWindow=�64   .��5      �� ���#B0B0B0$��1q�6#   
�������._�5   
   � $��15q�6    � RowDefinitions 5	      �� 5
      $
��Auto �6    �� 5      $
��Auto �6    �� 5      $
��Auto �6    �� 5      $
��Auto �6    .+�5      �� $��0��6   $��Bağlanılacak IP Adresi:��6%   $��0,0,0,5q�6D   ���#3336U   �� 5      -   5         ��Name$ IpAddressTextBox��5   #   $��1��6   $	��250��6=   $��0��6I   .��6V    � 	Resources 5      	 �� 
TargetType
 �� Property �� Value��%^   '��       �� 5      "	 ��6   .�5      �� 
 �@��CornerRadius$ 3��69   5      5      .��6%   5      �� 5      -   5         ��Name$ ErrorTextBlock��5   %   $��2��6   ���#D32F2F$��11(�6R   $��0,2,0,10q�6`   $
��Wrap=�6r   �� 5      $��3��6    �� Orientation$ 
Horizontal=�6&   $��Right=�6?   $��0,10,0,0q�6[   .+�5      �� -   5        $��Bağlan��5       �� 	IsDefault$
 True��6F   $��1��6W   $
��10,5q�6d    5      ��%^   '��       �� 5      "	 ��6    .�5      �� 
 �@��CornerRadius$ 3��6=   5      5       5!      �� 5"      $��İptal��6    �� IsCancel$
 True��6)   $��2��69   $��10,0,0,0q�6F   $
��10,5q�6X    5#      ��%^   '��       �� 5$      "	 ��6    .�5%      �� 
 �@��CornerRadius$ 3��6=   5&      5'      5(      5)      5*      5+      5,      