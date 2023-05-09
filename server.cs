if($RTB::Hooks::ServerControl)
{ 
	RTB_registerPref("Enabled",	"CRC Checker","$Pref::Server::CRCUpdater::Enabled",	"bool","Server_CRCUpdater","1","0","0","CRC_Enable");
	RTB_registerPref("Enabled",	"Notify server","$Pref::Server::CRCUpdater::NotifyServer",	"bool","Server_CRCUpdater","0","0","0","CRC_Notify");
	RTB_registerPref("Folder Name/Path",	"CRC Checker","$Pref::Server::CRCUpdater::FolderName", "string 999","Server_CRCUpdater","weapon_gun","0","0","CRC_Path");
	RTB_registerPref("Schedule Tick (seconds)",	"CRC Checker","$Pref::Server::CRCUpdater::SchedTime", "int 1 60","Server_CRCUpdater","5","0","0","CRC_TimeUpdate");	
}
else 
{
	if($Pref::Server::CRCUpdater::Enabled $= "") $Pref::Server::CRCUpdater::Enabled = true;
	if($Pref::Server::CRCUpdater::NotifyServer $= "") $Pref::Server::CRCUpdater::NotifyServer = false;
	if($Pref::Server::CRCUpdater::FolderName $= "") $Pref::Server::CRCUpdater::FolderName = "weapon_gun";	
	if($Pref::Server::CRCUpdater::Path $= "") $Pref::Server::CRCUpdater::Path = "add-ons/weapon_gun/*.cs";
	if($Pref::Server::CRCUpdater::SchedTime $= "") $Pref::Server::CRCUpdater::SchedTime = 5;	
}

function CRC_TimeUpdate(%oldVal,%newVal)
{
	if(%newVal < 5)
	messageAll('', "\c0It is recommended to not go lower than 5 seconds if the folder has a lot of scripts");
}

function CRC_Enable(%oldVal, %newVal)
{
	switch(%newVal)
	{
		case true: 	CRC_Update();
					CRC_Check();
										
		case false: cancel($CRCUpdaterSched);
	}
}

function CRC_Path(%oldVal, %newVal)
{
	$Pref::Server::CRCUpdater::Path = "add-ons/" @ %newVal @ "/*.cs";	
	
	if(findFirstFile($Pref::Server::CRCUpdater::Path) $= "")
	{
		messageAll('', "\c0The folder is invalid or does not exist, returning to the previous folder");
		$Pref::Server::CRCUpdater::Path = "add-ons/" @ %oldVal @ "/*.cs";
		$Pref::Server::CRCUpdater::FolderName = %oldVal;
	}
			
	CRC_Update();
	CRC_Check();
}

function CRC_Update()
{
	for(%CRCFile = findFirstFile($Pref::Server::CRCUpdater::Path); %CRCFile !$= ""; %CRCFile = findNextFile($Pref::Server::CRCUpdater::Path))
	$LastTimeChecked[%a++] = getFileCRC(%CRCFile);
}

function CRC_Check()
{
	if(!$Pref::Server::CRCUpdater::Enabled) return;

	for(%scriptfile = findFirstFile($Pref::Server::CRCUpdater::Path); %scriptfile !$= ""; %scriptfile = findNextFile($Pref::Server::CRCUpdater::Path))
	{
		%currentTimeChecked[%b++] = getFileCRC(%scriptfile);
		
		if($LastTimeChecked[%b] !$= %currentTimeChecked[%b])
		{
			$LastTimeChecked[%b] = %currentTimeChecked[%b];
			
			if($Pref::Server::CRCUpdater::NotifyServer) talk("File changed, executing" SPC %scriptfile);
			exec(%scriptfile);
			%fileschanged = true;
		}
	}

	cancel($CRCUpdaterSched);
	$CRCUpdaterSched = schedule($Pref::Server::CRCUpdater::SchedTime*1000,0,CRC_Check);
}

CRC_Update();
CRC_Check();