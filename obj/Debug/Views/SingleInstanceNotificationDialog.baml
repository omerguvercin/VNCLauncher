   M S B A M L   `   `   `  ����  VNCLauncher8    2VNCLauncher.Views.SingleInstanceNotificationDialog   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	     � Title$  Bilgi��5   	   $	��180��5   	   $	��350��6    � WindowStartupLocation$ CenterOwner=�5	   	    � WindowStyle$ 
ToolWindow=�5
   	    � 
ResizeMode$ NoResize=�5   	    � ShowInTaskbar$ False��5   	   ���#F0F0F0.��5      �� ���#2196F3$��1q�6#   
�������$��10q�6J    �� CornerRadius$ 5��6V   ._�5   
   �  � RowDefinitions 5      �� 5      $��* �6    �� 5      $
��Auto �6    .+�5      �� -   5         ��Name$ txtNotificationMessage��5      $��0��5      $
��Wrap=�5      $��20q�5      $��14(�5      ���#333$��Center=�5      $��Center=�5      $#��Uygulama zaten çalışıyor.��5      �� 5"      -   5        $��1��5"      $��Tamam��5#       �� 	IsDefault$
 True��5$      $��Right=�5&      $��	0,0,20,20q�5'      $
��15,5q�5(      $��14(�5)      ���#2196F3
�������$��0q�5,      	 � Cursor$
	 Hand��5-      
 � 	Resources
 5.       �� 
TargetType �� Property �� Value��%^   '��       �� 5/      " ��6   .�50      ��  �@��CornerRadius$ 3��69   51      52      53      54      55      56      