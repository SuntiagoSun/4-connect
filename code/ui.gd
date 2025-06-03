extends CanvasLayer
"""КОД ИНТЕРФЕЙСА"""
var win_once=true                               #Состояние победы
"""Функция управления видимостью интерфейса. Внутри функция проверяет, закончена
ли игра и выводит на экран победную надпись, а также позволяет выйти в главное меню.
Вызывается каждый кадр"""
func _process(delta: float) -> void:
	if Input.is_action_just_pressed("esc"):                        #Если нажата клавиша "esc" 
		get_tree().change_scene_to_file("res://Levels/menu.tscn")  #Загрузить главное меню
	if Globals.game_over and win_once:        #Если игра закончена, единожды выполнить следующий код
		$Timer.start()
		win_once=false
		if Globals.win==1:                    #Если победил зелёный
			$Win/Label.text="GREEN WINS!!!"   #Вывести победный текст
			$Win/Label.label_settings.font_color=Color(0,1,0,1)
		elif Globals.win==-1 and Globals.pvp_mode:   #Если победил красный
			$Win/Label.text="RED WINS!!!"            #Вывести победный текст
			$Win/Label.label_settings.font_color=Color(1,0,0,1)
		elif Globals.win==0:                  #Если вышла ничья
			$Win/Label.text="DRAW"            #Вывести победный текст
			$Win/Label.label_settings.font_color=Color(1,1,1,0.75)
		elif Globals.win==-1:                    #Если победил компьютер
			$Win/Label.text="COMPUTER WINS!!!"   #Вывести победный текст
			$Win/Label.label_settings.font_color=Color(0,0,1,1)
"""Функция завершения игры. Внутри делает возможным вернуться в 
главное меню после заврешения игры нажатием на любую клавишу.
Вызывается при нажатии на любую клавишу"""
func _input(event: InputEvent) -> void:
	#Если нажата клавиша на клавиатуре или левая кнопка мыши, а также победный текст выведен
	if (event is InputEventKey or Input.is_action_just_pressed("click")) and $Win.visible:
		get_tree().change_scene_to_file("res://Levels/menu.tscn")     #Загрузить главное меню
"""Функция смены видимости интерфейса. Делает победный текст видимым.
Вызывается по истечении таймера, после завершения игры"""
func _on_timer_timeout() -> void:
	$Win.visible=true                #Сделать текст видимым
