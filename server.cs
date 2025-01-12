if($Pref::Server::CRCUpdater::Path $= "") $Pref::Server::CRCUpdater::Path = "add-ons/weapon_gun/*.cs";
$CRC::FileList = "";
$CRC::FileCount = 0;

function CRC_logChange(%command)
{
    %lineShowCount = 0;
    
    // No need to check directory - TorqueScript will create it if needed
    if(!isObject(CRCFileObject))
        new FileObject(CRCFileObject);
        
    if(!isObject(CRCConsoleLogger))
        new ConsoleLogger(CRCConsoleLogger, "config/crcLog.out");
    
    CRCFileObject.openForWrite("config/crcLog.out");
    CRCFileObject.close();
    
    CRCConsoleLogger.attach();
    eval(%command); // Execute once
    CRCConsoleLogger.detach();
    
    // Store last line to check for duplicates
    %lastLine = "";
    
    CRCFileObject.openForRead("config/crcLog.out");
    while(!CRCFileObject.isEOF())
    {
        %line = CRCFileObject.readLine();
        if(trim(%line) $= "" || 
           getSubStr(%line, 0, 11) $= "BackTrace:" || 
           getSubStr(%line, 0, 9) $= "ResManager" ||
           %line $= %lastLine) // Skip if identical to last line
            continue;
            
        if(%lineShowCount < 500)
        {
            messageAll('', '<color:999999><font:consolas:18>CONSOLE: %1', strReplace(%line, "\t", "^"));
            %lineShowCount++;
        }
        %lastLine = %line;
    }
    CRCFileObject.close();
}

function CRC_Update()
{
    $CRC::FileList = "";
    $CRC::FileCount = 0;
    
    for(%CRCFile = findFirstFile($Pref::Server::CRCUpdater::Path); %CRCFile !$= ""; %CRCFile = findNextFile($Pref::Server::CRCUpdater::Path))
    {
        $CRC::File[$CRC::FileCount] = %CRCFile;
        $CRC::LastCRC[$CRC::FileCount] = getFileCRC(%CRCFile);
        $CRC::FileCount++;
    }   
}

function CRC_Check()
{
    for(%i = 0; %i < $CRC::FileCount; %i++)
    {
        %currentCRC = getFileCRC($CRC::File[%i]);
        
        if($CRC::LastCRC[%i] !$= %currentCRC)
        {
            $CRC::LastCRC[%i] = %currentCRC;
            CRC_logChange("exec(\"" @ $CRC::File[%i] @ "\");");
        }
    }

    cancel($CRCUpdaterSched);
    $CRCUpdaterSched = scheduleNoQuota(5000, 0, "CRC_Check");
}

CRC_Update();
CRC_Check();