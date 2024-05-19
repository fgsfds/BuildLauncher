import { useEffect } from 'react';
import './App.css';

function App() {

    useEffect(() => {
    }, []);

    return (
        <div>
            <img src="logo.png" width="300"></img>
            <h1 id="tabelLabel">BuildLauncher</h1>
            <h4 id="tabelLabel">Frontend for the Build Engine games and mods</h4>

            <div>
                <a className="big-font" href="https://github.com/fgsfds/BuildLauncher/releases/latest">
                    Download from GitHub
                </a>
            </div>

            <div>

            <br/>
            <br/>

                <a href="https://github.com/fgsfds/BuildLauncher">
                    <img className="with-margin" src="/github.png" height="100"></img>
                </a>

                <a href="https://discord.gg/mWvKyxR4et">
                    <img className="with-margin" src="/discord.png" height="100"></img>
                </a>
                
            </div>

        </div>
    );
}

export default App;