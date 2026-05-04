import './App.css'
import './index.css'
import {useEffect, useMemo, useState} from "react";

export default function App() {
    const [data, setData] = useState<AddonsDatabase | null>(null);
    const [currentList, setCurrentList] = useState<AddonEntry[]>([]);

    const url = "https://raw.githubusercontent.com/fgsfds/BuildLauncher/refs/heads/master/db/addons.json";

    function onButtonPressed(a: string) {
        setCurrentList(data![a].sort((x, y) => x.Title.localeCompare(y.Title)));
    }

    useEffect(() => {
        fetch(url)
            .then((res) => res.json())
            .then((json: AddonsDatabase) => {
                setData(json);
            });
        console.log("GOT JSON");
    }, []);

    if (!data) {
        return (
            <div>Loading...</div>
        );
    } else {
        return (
            <>
                {<Header onButtonPressed={onButtonPressed}/>}
                {<AddonsList addons={currentList}/>}
            </>
        );
    }
}


function Header({onButtonPressed}: { onButtonPressed: (a: string) => void }) {
    return (
        <>
            <div style={{display: "flex", flexDirection: "row", gap: "30px", margin: "30px"}}>
                <button onClick={() => onButtonPressed("Duke3D")}>Duke 3D</button>
                <button onClick={() => onButtonPressed("Blood")}>Blood</button>
                <button onClick={() => onButtonPressed("Wang")}>Shadow Warrior</button>
                <button onClick={() => onButtonPressed("Fury")}>Ion Fury</button>
            </div>
        </>
    )
}

function AddonsList({addons}: { addons: AddonEntry[] }) {
    const tcs: AddonEntry[] = [];
    const maps: AddonEntry[] = [];
    const mods: AddonEntry[] = [];

    addons.forEach((addon) => {
        if (addon.AddonType === "TC") {
            tcs.push(addon);
        } else if (addon.AddonType === "Map") {
            maps.push(addon);
        } else if (addon.AddonType === "Mod") {
            mods.push(addon);
        }
    });

    return (
        <div style={{display: 'flex', gap: '20px'}}>
            <div>{tcs.length > 0 && <GetList header={"TCs"} list={tcs}/>}</div>
            <div>{maps.length > 0 && <GetList header={"Maps"} list={maps}/>}</div>
            <div>{mods.length > 0 && <GetList header={"Mods"} list={mods}/>}</div>
        </div>
    );
}


function GetList({header, list}: { header: string, list: AddonEntry[] }) {
    return (
        <section style={{padding: "0 20px"}}>
            <h3 style={{textAlign: 'left', marginBottom: '40px'}}>{header}</h3>
            <ul style={{listStyleType: 'none', textAlign: 'left', padding: 0, margin: 0}}>
                {list.map((addon, index) => (
                    <AddonItem key={addon.Id || `${addon.Title}-${index}`} addon={addon}/>
                ))}
            </ul>
        </section>
    );
}

function AddonItem({addon}: { addon: AddonEntry }) {
    const formattedDate = useMemo(() => {
        const dateObj = new Date(addon.UpdateDate);
        return new Intl.DateTimeFormat('en-GB', {
            day: '2-digit',
            month: '2-digit',
            year: '2-digit',
        }).format(dateObj).replace(/\//g, '.');
    }, [addon.UpdateDate]);

    return (
        <li style={{marginBottom: '20px'}}>
            <div style={{fontWeight: 'bold'}}>{addon.Title}</div>
            <div style={{fontSize: '0.9em', color: '#8f8f8f'}}>
                v{addon.Version} • {formattedDate}
            </div>
            <div>
                <a href={addon.DownloadUrl} target="_blank" rel="noopener">
                    Download
                </a>
            </div>
        </li>
    );
}


interface AddonEntry {
    Id: string;
    AddonType: "TC" | "Mod" | "Map";
    Title: string;
    Version: string;
    DownloadUrl: string;
    Description: string;
    UpdateDate: Date;
    Author: string;
}

type AddonsDatabase = Record<string, AddonEntry[]>;
