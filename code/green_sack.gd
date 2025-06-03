extends Area2D
"""КОД ДЛЯ МЕШКА С ЗЕЛЁНЫМИ ДИСКАМИ"""
var disk_sc=preload("res://Objects/disk.tscn")     #Ссылка на предзагруженную сцену диска
var cant=false                                     #Невозможность хода
var mouse_in:bool=false                            #Состояние мышки внутри объекта
"""Функция спавна дисков для совершения хода зелёного игрока. Внутри функция 
добавляет один диск на сцену на позицию мышки, если сейчас ход зелёного игрока,
а также не даёт ему взять несколько дисков одновременно. Вызывается каждый кадр"""
func _process(delta: float) -> void:
	#Если игрок кликнул на этот объект, игра не закончена и он может ходить
	if Input.is_action_just_pressed("click") and mouse_in and not cant and not Globals.game_over:
		var disk=disk_sc.instantiate()                    #Загружаем диск на сцену
		$"../Disks".add_child(disk)
		disk.global_position=get_global_mouse_position()  #Задаём диску позицию и необходимые свойства
		disk.modulate=Color(0,1,0,1)                      #Ставим цвет диска зелёный                      
		disk.player=1
		disk.toogled=true
		disk.physics_material_override.bounce=0
		disk.gravity_scale=0.1
	#Если на сцене уже есть динамичный диск, или сейчас не ход зелёного, или игра уже закончилась
	if $"../Disks".get_child_count()!=0 or Globals.turn==-1 or Globals.game_over:
		modulate=Color(0.5,0.5,0.5,0.75)
		cant=true                                          #Не даём взять диск
	else:
		modulate=Color(1,1,1,1)
		cant=false                                         #Иначе даём взять диск
"""Функция определения положения курсора мышки. Если курсор находится
в области мешка, меняет переменную mouse_in"""
func _on_mouse_entered() -> void:
	mouse_in=true
"""Функция определения положения курсора мышки. Если курсор находится
вне области мешка, меняет переменную mouse_in"""
func _on_mouse_exited() -> void:
	mouse_in=false
