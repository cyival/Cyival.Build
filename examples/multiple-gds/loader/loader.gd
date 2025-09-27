class_name PackLoader
extends Node

const LOAD_PATH: String = "res://loads"
const FILE_NAME: String = "pack.json"

var loaded_packs: Array[PackDescriptor] = []

func load_packs() -> void:
	for i in ResourceLoader.list_directory(LOAD_PATH):
		if not i.ends_with("/"):
			continue
		
		var path = LOAD_PATH.path_join(i).path_join(FILE_NAME)
		if ResourceLoader.exists(path):
			var json = (ResourceLoader.load(path) as JSON).data as Dictionary
			
			if not json.has("id"):
				continue
			
			var descriptor = PackDescriptor.new()
			descriptor.id = json["id"]
			descriptor.path = LOAD_PATH.path_join(i)
			
			loaded_packs.append(descriptor)

func get_pack_id_json(id: String) -> JSON:
	var descriptor = get_descriptor_by_id(id)
	var path = descriptor.path.path_join(FILE_NAME)
	
	return ResourceLoader.load(path) as JSON

func get_descriptor_by_id(id: String) -> PackDescriptor:
	for i in loaded_packs:
		if i.id == id:
			return i
	
	return null
