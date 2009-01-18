del metageta.db3
echo .read  ../MetaGeta.DataStore/database/create_database.sql > temp_command
echo .quit >> temp_command
sqlite3 metageta.db3 < temp_command
del temp_command