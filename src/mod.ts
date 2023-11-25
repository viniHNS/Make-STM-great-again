import { DependencyContainer } from "tsyringe";
import { IPostDBLoadMod } from "@spt-aki/models/external/IPostDBLoadMod";
import { DatabaseServer } from "@spt-aki/servers/DatabaseServer";
import { IPreAkiLoadMod } from "@spt-aki/models/external/IPreAkiLoadMod";
import { ILogger } from "@spt-aki/models/spt/utils/ILogger";
import { LogTextColor } from "@spt-aki/models/spt/logging/LogTextColor";

class Mod implements IPostDBLoadMod, IPreAkiLoadMod
{
    preAkiLoad(container: DependencyContainer): void 
    {
        // get the logger from the server container
        const logger = container.resolve<ILogger>("WinstonLogger");
        logger.logWithColor("Making the STM-9 great again!", LogTextColor.YELLOW);
    }

    public postDBLoad(container: DependencyContainer): void 
    {
        // get database from server
        const databaseServer = container.resolve<DatabaseServer>("DatabaseServer");

        // Get all the in-memory json found in /assets/database
        const tables = databaseServer.getTables();

        // Find the STM-9 item by its Id
        const stm9 = tables.templates.items["60339954d62c9b14ed777c06"];
        
        // change the fire mode to single and full auto
        stm9._props.weapFireType = [
            "single",
            "fullauto"
        ];

        // increases ergonomics
        stm9._props.Ergonomics = 65;

        // decreases heat factor
        stm9._props.HeatFactor = 0.9;

        // change the firerate
        stm9._props.bFirerate = 700;
    }
}

module.exports = { mod: new Mod() }