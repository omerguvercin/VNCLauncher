   M S B A M L   `   `   `  ����F  BVNCLauncher, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null5clr-namespace:VNCLauncher.ViewsVNCLauncher.Views  5   	   +    %VNCLauncher.Views.DeleteSuccessDialog   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   1localclr-namespace:VNCLauncher.Views     5   	     � Title$  Bilgi��5   	   $	��200��6   $	��400��6$    � WindowStartupLocation$ CenterOwner=�60   
������  � 
ResizeMode$ NoResize=�5	   "    � WindowStyle$
 None=�68    � AllowsTransparency$
 True��5
   	    � 	Resources 5       
  FadeIn �� TargetProperty W� From	 W� To	 g� Duration  FadeOut��%�   &	        &	 d     �� 5   
   .��5      W� $ Opacity�6    06B    16K   $	 0:0:0.3D�6R   5      �� 5   
   .��5      W� $ Opacity�6    16B    06K   $	 0:0:0.3D�6R   5      5      .��5      �� ���#F5F5F5
 �� CornerRadius$
 10��6"   $��10q�64    J� Effect 5   
   3  -System.Windows.Media.Effects.DropShadowEffect  5         
BlurRadius$ 15Q�6      ShadowDepth$ 1Q�6/      Color$ 	#40000000��6?   ._�5   
   � $��20q�6    � RowDefinitions 5      �� 5      $
��Auto �6    �� 5      $��* �6    .+�5       �� -   5         ��Name$ MessageTextBlock��5       $��0��62   $
��Wrap=�5!      $��0,0,0,20q�6+   $��14(�5"      ���#3333336%   �� 5$      -   5        $��1��5$      $��Tamam��6"   $	��100��62   $��30��6>   $��Center=�5%      $��Bottom=�62   ���#007ACC
�������$��0q�5'      $
��10,5q�6)   ��5)      �� 5*       �� 
TargetType" ��6   .�5+      ��  �� Property � �� �� Value 5,      �� 5-   "    �� 
TargetType" ��62   .R�5.   &   �� #��z��6-   $
 5��5/   -   #��z��50   -   #��z��51   -   ._�52   *   �� $��Center=�6;   $��Center=�53   9   54   '   55   #   57       �� Triggers 58      P� 59       P� Property �@J�IsMouseOver P� Value$
 True��6=   . �5:   "   ��  � �� �#0059996?   5;      5=      5?      5@      5A      5B      