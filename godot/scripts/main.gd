extends Node2D
## Fully playable touch-first 90-second vertical slice.
## Touch or mouse: drag anywhere to steer. Keyboard: WASD / arrows.
const DURATION := 90.0
const HERO_SPEED := 280.0
const ENEMY_CAP := 18
const WIDTH := 720.0
const HEIGHT := 1280.0
var rng := RandomNumberGenerator.new()
var state := "menu"
var elapsed := 0.0
var score := 0
var best := 0
var lives := 3
var hero := Vector2(360, 1020)
var hero_inv := 0.0
var joker := 0.0
var touch_id := -1
var target := Vector2.ZERO
var dragging := false
var enemies: Array[Dictionary] = []
var pickups: Array[Dictionary] = []
var sparks: Array[Dictionary] = []
var spawn_timer := 0.0
var pickup_timer := 0.0
var sound_on := true
var sound_player: AudioStreamPlayer
var master_bus := 0
var font: Font
var pulse := 0.0
var shake := 0.0
var status_timer := 0.0
var status_text := ""
var menu_button := Rect2(145, 780, 430, 100)
var pause_button := Rect2(610, 28, 84, 75)
var resume_button := Rect2(145, 770, 430, 100)
var restart_button := Rect2(145, 890, 430, 100)
var toggle_button := Rect2(145, 1020, 430, 85)

func _ready() -> void:
	rng.randomize()
	font = ThemeDB.fallback_font
	sound_player = AudioStreamPlayer.new()
	add_child(sound_player)
	sound_player.bus = "Master"
	load_settings()
	set_process(true)
	queue_redraw()

func load_settings() -> void:
	var f := FileAccess.open("user://neon_save.json", FileAccess.READ)
	if f:
		var v = JSON.parse_string(f.get_as_text())
		if typeof(v) == TYPE_DICTIONARY:
			best = int(v.get("best", 0))
			sound_on = bool(v.get("sound", true))

func save_settings() -> void:
	var f := FileAccess.open("user://neon_save.json", FileAccess.WRITE)
	if f:
		f.store_string(JSON.stringify({"best": best, "sound": sound_on}))

func start_game() -> void:
	state = "playing"
	elapsed = 0
	score = 0
	lives = 3
	joker = 0
	hero_inv = 1.5
	hero = Vector2(WIDTH * .5, HEIGHT * .8)
	enemies.clear()
	pickups.clear()
	sparks.clear()
	spawn_timer = 0
	pickup_timer = 0
	dragging = false
	touch_id = -1
	show_status("SURVIVE 90 SECONDS")
	sfx("start")
	queue_redraw()

func show_status(msg: String) -> void:
	status_text = msg
	status_timer = 2.0

func _process(delta: float) -> void:
	var dt: float = minf(delta, .04)
	pulse += dt
	status_timer = maxf(0, status_timer - dt)
	shake = maxf(0, shake - dt * 25)
	if state != "playing":
		queue_redraw()
		return
	elapsed += dt
	hero_inv = maxf(0, hero_inv - dt)
	joker = maxf(0, joker - dt)
	var direction := Vector2.ZERO
	if dragging:
		var diff := target - hero
		if diff.length() > 14:
			direction = diff.normalized() * minf(1.0, diff.length() / 65.0)
	direction += Input.get_vector("ui_left", "ui_right", "ui_up", "ui_down")
	if direction.length() > 1:
		direction = direction.normalized()
	hero += direction * HERO_SPEED * dt
	hero.x = clampf(hero.x, 25, WIDTH - 25)
	hero.y = clampf(hero.y, 110, HEIGHT - 35)
	spawn_timer += dt
	pickup_timer += dt
	if spawn_timer >= maxf(.65, 1.5 - elapsed / 240):
		spawn_timer = 0
		if enemies.size() < ENEMY_CAP:
			spawn_enemy()
	if pickup_timer >= 3.4:
		pickup_timer = 0
		spawn_pickup()
	for i in range(enemies.size() - 1, -1, -1):
		var e := enemies[i]
		var p: Vector2 = e["pos"]
		var v: float = e["speed"]
		var kind: int = e["kind"]
		if kind == 0:
			p += (hero - p).normalized() * v * dt
		elif kind == 1:
			p += Vector2(sin(pulse * 2.5 + float(e["seed"])) * 65, 100) * dt
		else:
			p += (hero - p).normalized() * v * .65 * dt
		e["pos"] = p
		if p.distance_to(hero) < 34:
			if joker > 0:
				burst(p, Color("#baff72"), 18)
				enemies.remove_at(i)
				score += 15
				sfx("blast")
			elif hero_inv <= 0:
				lives -= 1
				hero_inv = 1.4
				shake = 9
				burst(hero, Color("#ff648c"), 22)
				sfx("hurt")
				if lives <= 0:
					finish(false)
					break
		elif p.y > HEIGHT + 100:
			enemies.remove_at(i)
	for i in range(pickups.size() - 1, -1, -1):
		var item := pickups[i]
		var p: Vector2 = item["pos"]
		if p.distance_to(hero) < 36:
			if item["kind"] == 1:
				joker = 7.0
				hero_inv = maxf(hero_inv, .15)
				show_status("JOKER! 7 SECONDS OF POWER")
				burst(hero, Color("#baff72"), 42)
				sfx("power")
			else:
				score += 10
				burst(p, Color("#ffdb7a"), 14)
				sfx("collect")
			pickups.remove_at(i)
	for i in range(sparks.size() - 1, -1, -1):
		var particle := sparks[i]
		particle["pos"] += particle["velocity"] * dt
		particle["velocity"] *= .95
		particle["life"] -= dt
		if particle["life"] <= 0:
			sparks.remove_at(i)
	if elapsed >= DURATION and state == "playing":
		finish(true)
	queue_redraw()

func spawn_enemy() -> void:
	var edge := rng.randi_range(0, 2)
	var p := Vector2(rng.randf_range(30, WIDTH - 30), -40)
	if edge == 1:
		p = Vector2(-35, rng.randf_range(160, HEIGHT * .7))
	elif edge == 2:
		p = Vector2(WIDTH + 35, rng.randf_range(160, HEIGHT * .7))
	enemies.append({"pos": p, "speed": rng.randf_range(90, 130), "kind": rng.randi_range(0, 2), "seed": rng.randf_range(0, 6.28)})

func spawn_pickup() -> void:
	var p := Vector2(rng.randf_range(65, WIDTH - 65), rng.randf_range(230, HEIGHT - 170))
	pickups.append({"pos": p, "kind": 1 if rng.randf() < .26 else 0})
	if pickups.size() > 12:
		pickups.pop_front()

func burst(pos: Vector2, color: Color, count: int) -> void:
	for i in count:
		var angle := rng.randf_range(0, TAU)
		var speed := rng.randf_range(80, 340)
		sparks.append({"pos": pos, "velocity": Vector2.RIGHT.rotated(angle) * speed, "life": rng.randf_range(.25, .65), "color": color})

func finish(won: bool) -> void:
	state = "won" if won else "lost"
	if score > best:
		best = score
	save_settings()
	sfx("win" if won else "hurt")
	dragging = false

func _input(event: InputEvent) -> void:
	if event is InputEventScreenTouch:
		var p := screen_to_game(event.position)
		if event.pressed:
			if state == "playing" and pause_button.has_point(p):
				state = "paused"
				dragging = false
				queue_redraw()
				return
			if state == "playing" and touch_id == -1:
				touch_id = event.index
				dragging = true
				target = p
			elif state != "playing":
				press_menu(p)
		elif event.index == touch_id:
			touch_id = -1
			dragging = false
	elif event is InputEventScreenDrag:
		if event.index == touch_id:
			target = screen_to_game(event.position)
	elif event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT:
		var p := screen_to_game(event.position)
		if event.pressed:
			if state == "playing" and pause_button.has_point(p):
				state = "paused"
				dragging = false
			elif state == "playing":
				dragging = true
				target = p
			else:
				press_menu(p)
		else:
			dragging = false
	elif event is InputEventMouseMotion and dragging:
		target = screen_to_game(event.position)
	elif event is InputEventKey and event.pressed and not event.echo:
		if event.keycode == KEY_ESCAPE:
			if state == "playing":
				state = "paused"
			elif state == "paused":
				state = "playing"
		elif event.keycode == KEY_SPACE and state != "playing":
			press_menu(Vector2(360, 820))

func screen_to_game(p: Vector2) -> Vector2:
	return get_viewport().get_canvas_transform().affine_inverse() * p

func press_menu(p: Vector2) -> void:
	if state == "menu":
		if menu_button.has_point(p):
			start_game()
		elif toggle_button.has_point(p):
			sound_on = not sound_on
			save_settings()
	elif state == "paused":
		if resume_button.has_point(p):
			state = "playing"
		elif restart_button.has_point(p):
			start_game()
		elif toggle_button.has_point(p):
			sound_on = not sound_on
			save_settings()
	elif state == "won" or state == "lost":
		if restart_button.has_point(p) or menu_button.has_point(p):
			start_game()
	queue_redraw()

func _draw() -> void:
	var canvas := Vector2(WIDTH, HEIGHT)
	draw_rect(Rect2(Vector2.ZERO, canvas), Color("#05091b"))
	# Layered animated grid floor with depth, stars, ambient bands.
	for i in 46:
		var px := fposmod(float(i * 137 + 41), WIDTH)
		var py := fposmod(float(i * 211 + 93) + pulse * (9 + i % 4), HEIGHT)
		draw_circle(Vector2(px, py), 1.5 + (i % 3) * .5, Color(0.45, .77, 1, .2 + .18 * sin(pulse + i)))
	for i in 17:
		var y := HEIGHT * .35 + pow(float(i) / 16, 1.9) * HEIGHT * .67
		draw_line(Vector2(0, y), Vector2(WIDTH, y), Color("#183d60").darkened(float(i) / 24), 1.5)
	for i in 12:
		var x := float(i) / 11 * WIDTH
		draw_line(Vector2(WIDTH * .5, HEIGHT * .35), Vector2(x, HEIGHT), Color("#173f62"), 1)
	for i in 7:
		var band_y := fposmod(float(i) * 210 + pulse * 25, HEIGHT + 300) - 150
		draw_line(Vector2(0, band_y), Vector2(WIDTH, band_y - 60), Color(0.2, .6, .85, .08), 22)
	if shake > 0:
		draw_circle(hero, 40 + shake, Color(1, .2, .3, .06))
	for item in pickups:
		var p: Vector2 = item["pos"]
		var power: bool = item["kind"] == 1
		var color := Color("#baff72") if power else Color("#ffdc80")
		draw_circle(p, 27 + sin(pulse * 5) * 4, Color(color, .13))
		draw_colored_polygon(PackedVector2Array([p + Vector2(0,-17), p + Vector2(14,0), p + Vector2(0,17), p + Vector2(-14,0)]), color)
		if power:
			draw_string(font, p + Vector2(-9, 8), "⚡", HORIZONTAL_ALIGNMENT_LEFT, -1, 23, Color("#112816"))
	for e in enemies:
		var p: Vector2 = e["pos"]
		var kind: int = e["kind"]
		var col: Color = [Color("#ff4e93"), Color("#ff9e63"), Color("#c38cff")][kind]
		draw_circle(p, 26, Color(col, .13))
		draw_arc(p, 18, 0, TAU, 12, col, 4)
		draw_circle(p, 9, col)
		draw_circle(p + Vector2(-4, -3), 2, Color("#160e2d"))
		draw_circle(p + Vector2(4, -3), 2, Color("#160e2d"))
	if state == "playing" or state == "paused":
		if joker > 0:
			draw_arc(hero, 38 + sin(pulse * 10) * 4, 0, TAU, 40, Color("#baff72"), 5)
			draw_string(font, hero + Vector2(-18, -48), "%.1fs" % joker, HORIZONTAL_ALIGNMENT_LEFT, -1, 20, Color("#dfff9e"))
		var alpha := .4 if hero_inv > 0 and int(pulse * 12) % 2 == 0 else 1.0
		draw_circle(hero, 30, Color(.2, .8, 1, .08))
		draw_colored_polygon(PackedVector2Array([hero + Vector2(0,-25),hero + Vector2(20,-8),hero + Vector2(17,18),hero + Vector2(0,25),hero + Vector2(-17,18),hero + Vector2(-20,-8)]), Color(.2, .85, 1, alpha))
		draw_circle(hero + Vector2(0,1), 11, Color("#09142c"))
		draw_circle(hero + Vector2(-5,-2), 2.8, Color.WHITE)
		draw_circle(hero + Vector2(5,-2), 2.8, Color.WHITE)
	for spark in sparks:
		var a: float = clampf(float(spark["life"]) * 2, 0, 1)
		draw_circle(spark["pos"], 4 * a, Color(spark["color"], a))
	if state == "playing":
		draw_string(font, Vector2(22, 48), "♥ %d     ◆ %d     BEST %d" % [lives, score, best], HORIZONTAL_ALIGNMENT_LEFT, -1, 26, Color("#e4faff"))
		draw_string(font, Vector2(22, 90), "TIME  %02d / 90" % int(elapsed), HORIZONTAL_ALIGNMENT_LEFT, -1, 21, Color("#8bd9f7"))
		draw_button(pause_button, "Ⅱ")
	if status_timer > 0 and state == "playing":
		draw_string(font, Vector2(90, 170), status_text, HORIZONTAL_ALIGNMENT_CENTER, 540, 26, Color("#d4ffff"))
	if state == "menu":
		draw_title("NEON ESCAPE ∞", "A NEW BEGINNING")
		draw_button(menu_button, "PLAY")
		draw_button(toggle_button, "SOUND: ON" if sound_on else "SOUND: OFF")
	elif state == "paused":
		draw_title("PAUSED", "TAKE YOUR TIME")
		draw_button(resume_button, "RESUME")
		draw_button(restart_button, "RESTART")
		draw_button(toggle_button, "SOUND: ON" if sound_on else "SOUND: OFF")
	elif state == "won":
		draw_title("ESCAPED!", "90 SECONDS SURVIVED  •  SCORE %d" % score)
		draw_button(restart_button, "PLAY AGAIN")
	elif state == "lost":
		draw_title("GAME OVER", "SCORE %d  •  BEST %d" % [score, best])
		draw_button(restart_button, "TRY AGAIN")

func draw_title(title: String, subtitle: String) -> void:
	draw_string(font, Vector2(55, 430), title, HORIZONTAL_ALIGNMENT_CENTER, 610, 64, Color("#c8f8ff"))
	draw_string(font, Vector2(65, 510), subtitle, HORIZONTAL_ALIGNMENT_CENTER, 590, 25, Color("#8dd2f3"))

func draw_button(area: Rect2, label: String) -> void:
	draw_rect(area, Color("#102745"))
	draw_rect(area, Color("#67eaff"), false, 3)
	draw_string(font, area.position + Vector2(20, 65), label, HORIZONTAL_ALIGNMENT_CENTER, area.size.x - 40, 30, Color("#efffff"))

func sfx(kind: String) -> void:
	# Synthesized placeholder sound: replace with licensed studio assets before release.
	if not sound_on:
		return
	var rates := {"collect": 880.0, "hurt": 120.0, "blast": 85.0, "power": 630.0, "start": 440.0, "win": 760.0}
	var freq: float = rates.get(kind, 300.0)
	var length := .22 if kind == "blast" else .12
	var mix_rate := 22050
	var count := int(mix_rate * length)
	var pcm := PackedVector2Array()
	pcm.resize(count)
	for i in count:
		var t := float(i) / mix_rate
		var fade := pow(1.0 - float(i) / count, 2)
		var tone := sin(TAU * freq * t * (1.0 - t * .6))
		var noise := rng.randf_range(-1, 1) * (.6 if kind == "blast" else .08)
		var value := (tone * .25 + noise * .5) * fade * .24
		pcm[i] = Vector2(value, value)
	var generator := AudioStreamGenerator.new()
	generator.mix_rate = mix_rate
	generator.buffer_length = .5
	sound_player.stream = generator
	sound_player.play()
	var playback := sound_player.get_stream_playback() as AudioStreamGeneratorPlayback
	if playback:
		playback.push_buffer(pcm)
