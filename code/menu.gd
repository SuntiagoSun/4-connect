extends Control
"""КОД МЕНЮ"""
var pvp_scene=preload("res://Levels/main_level.tscn")      #Ссылка на предзагруженную сцену главного уровня
"""Функция перехода к выбору режима. Выводит на экран варианты
выбора режима игры. Вызывается после нажатися кнопки 'ИГРАТЬ'"""
func _on_button_pressed() -> void:
	$VBoxContainer.visible=false
	$VBoxContainer2.visible=true
"""Функция выхода из игры. Закрывает игру. 
Вызывается после нажатися кнопки 'ВЫХОД"""
func _on_button_2_pressed() -> void:
	get_tree().quit()

"""Функция запуска игры игрока против игрока. Запускает игру
в режиме 'Игрок против игрока'.Вызывается после нажатися кнопки 'ИГРОК'"""
func _on_button_4_pressed() -> void:
	Globals.reset()
	Globals.pvp_mode=true
	get_tree().change_scene_to_packed(pvp_scene)

"""Функция запуска игры игрока против компьютера. Запускает игру
в режиме 'Игрок против компьютера'.Вызывается после нажатися кнопки 'КОМПЬЮТЕР'"""
func _on_button_5_pressed() -> void:
	Globals.reset()
	Globals.pvp_mode=false
	get_tree().change_scene_to_packed(pvp_scene)
	
"""Функция перехода к начальным кнопкам. Выводит на экран кнопки 'ИГРАТЬ' и
'ВЫЙТИ'. Вызывается после нажатися кнопки 'НАЗАД'"""
func _on_button_3_pressed() -> void:
	$VBoxContainer2.visible=false
	$VBoxContainer.visible=true
