create table datastore(
	datastore_id integer primary key, 
	name varchar,
	description varchar,
	template_name varchar
);

create table plugin_setting(
	datastore_id integer references datastore(datastore_id), 
	plugin_type_name varchar,
	setting_name varchar, 
	setting_value varchar
);

create table file(
	file_id integer primary key,
	datastore_id integer references datastore(datastore_id)
);
create table tag(
	file_id integer references file(file_id), 
	tag_name varchar, 
	tag_value varchar
);
