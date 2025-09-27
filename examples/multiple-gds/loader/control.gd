extends Control

@onready
var loader := $PackLoader

@onready
var tab_container := $HBoxContainer/VBoxContainer2/TabContainer

@onready
var id_view := $HBoxContainer/VBoxContainer2/TextEdit

const PCK_PATH: String = "packs"

func _ready() -> void:
	var path = get_running_path().path_join(PCK_PATH)
	var files = DirAccess.get_files_at(path)
	
	for i in files:
		if i.get_extension() != "pck":
			continue
		
		var pck_path = path.path_join(i)
		print("loading pack from ", pck_path)
		ProjectSettings.load_resource_pack(pck_path)
	

func get_running_path() -> String:
	if (OS.has_feature("editor")):
		return ProjectSettings.globalize_path("res://")
	
	return ProjectSettings.globalize_path(OS.get_executable_path().path_join(".."))

func _on_button_pressed() -> void:
	loader.load_packs()
	update_view()

func update_view():
	for i in tab_container.get_children():
		i.queue_free()
	
	id_view.text = ""
	
	for pack in loader.loaded_packs:
		id_view.text += pack.id + "\n"
		
		var text_edit = TextEdit.new()
		text_edit.editable = false
		text_edit.name = pack.id
		text_edit.text = JSON.stringify(loader.get_pack_id_json(pack.id).data)
		
		tab_container.add_child(text_edit)
