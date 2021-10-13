import React, {useState, useEffect} from 'react';
import { Grid } from '@material-ui/core';
import Header from "./components/Header.jsx";
import Authentication from "./components/Authentication.jsx";
import RegForm from "./registration/RegFormTest.jsx";
import Home from "./components/Home.jsx";

export default function App() {
  const [authenticated, setAuthenticated] = useState(false);
  const [userProfile, setUserProfile] = useState({});
  const [token, setToken] = useState(null);
   // wsEndpoint - IP адрес сокета, для обмена данными с клиентом
  const [wsEndpoint] = useState("ws://192.168.2.109:3120") //Local
  const [kfbRESTApi] = useState("http://192.168.2.150:5002") //Local KFB main REST

  return (
    <div>
      {/* страница авторизации  */}
      <div>
          {/* <Header/>
          <Authentication
            // VARS
            kfbRESTApi={kfbRESTApi}
            // FUNCTIONS
            setAuthenticated={setAuthenticated}
            setUserProfile={setUserProfile}
            setToken={setToken}
          /> */}
          {/* <RegForm/> */}
        </div>
      {authenticated === false ?
        <div>
          <Header/>
          <Authentication
            // VARS
            kfbRESTApi={kfbRESTApi}
            // FUNCTIONS
            setAuthenticated={setAuthenticated}
            setUserProfile={setUserProfile}
            setToken={setToken}
          />
        </div>
        :
        <Grid container>
          <Grid item xs={12}>
            <Home
              // VARS
              wsEndpoint={wsEndpoint}
              kfbRESTApi={kfbRESTApi}
              userProfile={userProfile}
              token={token}
              // FUNCTIONS 
              setAuthenticated={setAuthenticated}
              setUserProfile={setUserProfile}
            /> 
          </Grid>
        </Grid>
      }
    </div>
  );
}
