# Документация к лабораторной номер 1 #
### Возможности ###
1. Построение цветовой палитры с возможностью выбора цвета.
2. Переход между различными цветовыми моделями.
***

Алгоритмы переходов реализованы в следующих методах:
1. private System.Windows.Media.Color LabToRgb(string[] mas);
2. private bool HexToRgb(string str, out System.Windows.Media.Color? result_color);
3. private System.Windows.Media.Color HsvToRgb(string[] mas);

Все переходы реализуются через цветовую модель RGB.

Для реализации цветовой палитры использовалась библиотека ColorPicker.

