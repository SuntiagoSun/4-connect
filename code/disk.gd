extends RigidBody2D
"""КОД ДИСКА"""
var mouse_in: bool = false      #Состояние мышки внутри диска
var toogled: bool = false       #Состояние удержания диска игроком
var in_field: bool = false      #Находится ли диск в игровом поле
var active: bool = false        #Заморожен ли диск внутри поля
var player: int = 0             #Игрок, управляющий диском
"""Функция спрайта диска. Выбирает случайным образом спрайт диска.
Вызывается единожды при появлении диска на сцене"""
func _ready() -> void:
	$Sprite2D.frame=randi_range(0,5)
"""Функция контроля пермещения диска. Внутри проверяет, держит ли
Игрок диск мышкой и находится ли он за пределами поля.
Вызывается каждый кадр"""
func _process(delta: float) -> void:
	#Если игрок кликает на диск и он не в игровом поле
	if Input.is_action_just_pressed("click") and mouse_in and not in_field:
		toogled=true                        #Диск теперь удержан игроком
		physics_material_override.bounce=0
		gravity_scale=0.1
	elif Input.is_action_just_released("click"):          #Если игрок отпускает диск
		toogled=false                       #Диск теперь свободен
		physics_material_override.bounce=0.2
		gravity_scale=1

"""Функция следования диска за мышкой игрока. Внутри если диск удержан игроком,
двигает его в сторону курсора. Вызывается каждый кадр."""
func _physics_process(delta: float) -> void:
	#Если диск удержан
	if toogled:
		var mouse_position = get_global_mouse_position()
		var diff=mouse_position-global_position      #Рассчитываем вектор направления диска в сторону мыши
		linear_velocity=Vector2.ZERO
		apply_impulse(diff*100)                      #Даём диску импульс в сторону мыши
"""Функция выставления диска на игровом поле. Внутри замораживает диск на месте,
после чего передаёт главной сцене координаты диска, с помощью которых обновляется поле.
Вызывается, когда диск касается дна при падении внутри поля."""
func get_point():
	freeze_body($".")                                #Замараживаем диск
	active=true
	Globals.turn=-Globals.turn                       #Передаём ход
	var x_point=int(position.x/125)-3
	var y_point=int(position.y/125)-1
	var main_level=get_node("/root/MainLevel")
	main_level.UpdateField(y_point,x_point,player)   #Обновляем поле
	
"""Функция определения положения курсора мышки. Если курсор находится
в области диска, меняет переменную mouse_in"""
func _on_grab_area_mouse_entered() -> void:
	mouse_in=true

"""Функция определения положения курсора мышки. Если курсор находится
вне области диска, меняет переменную mouse_in"""
func _on_grab_area_mouse_exited() -> void:
	mouse_in=false

"""Функция определения коллизий диска с игровыми зонами. Внутри проверяет,
попал ли диск в зону поля и меняет в таком случае его свойства. Также
проверяет, вылетел ли диск за пределы карты. Вызывается при вхождении
диска в любую из зон."""
func _on_grab_area_area_entered(area: Area2D) -> void:
	if area.name=="DeathArea":                #Если диск вошёл в зону смерти
		queue_free()                          #Удаляем диск со сцены
	if area.name=="FieldArea":                #Если диск вошёл в зону поля
		in_field=true                         #Меняем свойства диска, чтобы его было невозможно подобрать
		linear_velocity=Vector2(0,1)
		toogled=false
		physics_material_override.bounce=0.01
		gravity_scale=0.5

"""Функция проверки коллизий диска с другими объектами. Внутри проверяет,
столкнулся ли диск с другим диском или со дном игрового поля, после чего
вызывает функцию выставления диска внутри игрового поля. Вызывается после
каждого контакта диска с другим объектом"""
func _on_body_entered(body: Node) -> void:
	if (body.name=="Disk" or body.name.substr(0,12)=="@RigidBody2D" or body.name.substr(0,13)=="@StaticBody2D"or body.name=="Bottom") and in_field:
		call_deferred("get_point")
"""Функция заморозки диска. Внутри удаляет со сцены динамичный диск, при 
этом копируя все его свойства в новый, статичный диск, который добавляется
на сцену. Вызывается каждый раз, когда диск выставляется на игровом поле"""
func freeze_body(body: RigidBody2D):
	var static_body = StaticBody2D.new()               #Создаём новый статичный диск
	static_body.transform = body.transform             #Копируем свойства
	static_body.collision_layer = body.collision_layer
	static_body.collision_mask = body.collision_mask
	static_body.modulate=body.modulate
	for child in body.get_children():                  #Копируем все дочерние элементы
		var dup = child.duplicate()
		static_body.add_child(dup) 
	$"../../StaticDisks".add_child(static_body)        #Добавляем статичный диск на сцену
	body.queue_free()                                  #Удаляем динамический диск со сцены
