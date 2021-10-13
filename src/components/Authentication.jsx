import React, { useState, useEffect } from "react";
import { makeStyles } from "@material-ui/core/styles";
import Container from "@material-ui/core/Container";
import CssBaseline from "@material-ui/core/CssBaseline";
import Typography from "@material-ui/core/Typography";
import TextField from "@material-ui/core/TextField";
import Checkbox from "@material-ui/core/Checkbox";
import FormGroup from "@material-ui/core/FormGroup";
import FormControlLabel from "@material-ui/core/FormControlLabel";
import FormControl from "@material-ui/core/FormControl";
import Button from "@material-ui/core/Button";
import VisibilityOff from "@material-ui/icons/VisibilityOff";
import IconButton from "@material-ui/core/IconButton";
import Visibility from "@material-ui/icons/Visibility";
import OutlinedInput from "@material-ui/core/OutlinedInput";
import InputAdornment from "@material-ui/core/InputAdornment";
import InputLabel from "@material-ui/core/InputLabel";
import Card from '@material-ui/core/Card';
import swal from "sweetalert";
var fetch = require('node-fetch');

//useSty - useStyle
//cls - class
//Btn - Button 
//#FFFFFF - white //#FFFAFA - snow //#e0e0e0 - Gray88 //#616161 - Granite Gray

const useStyles = makeStyles((theme) => ({
  typographySty: {
    border: "1px solid #212121",
    borderRadius:"10px", 
    background:"#546e7a"
  },
  card:{
    margin:'10px',
    borderRadius:'5px',
    border: "1px solid #424242",
    background:"#cfd8dc"
  },
  div:{
    padding:"1px", 
    margin:"12px", 
    border: "solid 2px #37474f",
    fontFamily:"Garamond", 
    color:'#455a64',
    textAlign:"center"
  },
  textField:{
    margin: "20px",
    width: "30ch",     
    border: "solid 1px #e0e0e0",
    background: "#FFFAFA",
    borderRadius:"10px",
  },
  formControl:{
    borderRadius:"10px",
    border: "solid 1px #e0e0e0",
    background:"#FFFAFA", 
    width: "30ch"
  },
  btn:{
    margin: "15px",
    color:"#FFFAFA",
    border:"solid 1px #455a64",
    backgroundColor:"#90a4ae",      
    marginTop:"30px",
    marginBottom:"20px",
    fontFamily:"Garamond",
    fontSize:14,
    width: "250px",
    textShadow:"DarkGreen 1px 1px"
  }
}));

export default function Authentication(props) {  
  const classes = useStyles();
  const [kfbRESTApi] = useState(props.kfbRESTApi) //KFB main REST
  const [showLogin, setShowLogin] = useState(false);
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [rememberMe, setRememberMe] = useState(true);
  const [error, setError] = useState(false);

  useEffect(()=>{
    console.log("AUTH", props)
    let token = localStorage.getItem("token")
    if(token){
      props.setToken(token)
      fetchUserProfile(token)
      console.log("TOKEN", token)
    }
    else{
      setShowLogin(true)
      console.log("TOKEN NOT FOUND")
    }
  },[])

  function handleLoginChange(event){
    console.log("LOGIN", event.target.value)
    setUsername(event.target.value)
    setError(false)
  }
  function handlePasswordChange(event){
    console.log("PASS", event.target.value)
    setPassword(event.target.value)
    setError(false)
  }
  function handleClickShowPassword(){
    console.log("SH PASS", !showPassword)
    setShowPassword(!showPassword)
  }
  function handleRememberMeChange(event){
    console.log("RM ME", event.target.checked)
    setRememberMe(event.target.checked)
  }
  async function LoginButtonClick() {
    let body = JSON.stringify({
          "userName": username,
          "password": password
        })
    await fetch(kfbRESTApi + "/api/users/Login",
      {
        "headers": { "content-type": "application/json" },
        "method": "POST",
        "body": body
      }
    )
    .then(response => response.json())
    .then(function(res){
        // console.log("RES", res)
        console.log("AUTH TOKEN", res.token)
        if(res.isAuthSuccessful === true){
          props.setToken(res.token)
          fetchUserProfile(res.token)
          setUsername("")
          setPassword("")
          if(rememberMe === true){
            localStorage.setItem("token", res.token)
          }
        }
        else{
          setError(true)
        }
      }
    );
  }
  //Регистрация пользователя
  async function RegistrationBtnClick() {
    let body = JSON.stringify({
          // "userName": username,
          // "password": password
          "firstName": null,
          "lastName": null,
          "userName": null,
          "email": null,
          "password": null,
          "confirmPassword": null
        })
    await fetch(kfbRESTApi + "​/api​/users​/Registration",                             
      {
        "headers": { "content-type": "application/json" },
        "method": "POST",
        "body": body
      }
    )
    .then(response => response.json())
    .then(function(res){
        // console.log("RES", res)
        console.log("AUTH TOKEN", res.token)
        if(res.isAuthSuccessful === true){
          props.setToken(res.token)
          fetchUserProfile(res.token)
          setUsername("")
          setPassword("")
          if(rememberMe === true){
            localStorage.setItem("token", res.token)
          }
        }
        else{
          setError(true)
        }
      }
    );
  }

  async function fetchUserProfile(token) {
    console.log("LOAD PROFILE")
    await fetch(kfbRESTApi + "/api/users/GetUserInfo",
      {
        "headers": {"Authorization": "Bearer " + token},
        "method": "GET"
      }
    )
    .then(response => response.json())
    .then(function(res){
        console.log("PROFILE", res)
        props.setUserProfile(res)
        props.setAuthenticated(true)
      }
    );
  }
  return (
    showLogin === true &&
      <React.Fragment>
        <CssBaseline />
        <Container maxWidth="xs" style={{paddingTop:"140px"}}>
          <Typography className={classes.typographySty}>
          <Card className={classes.card}>
            <div className={classes.div}>
              <h2>Вход в систему</h2>
            </div>
          
            <form
              noValidate
              autoComplete="off"
              align="center"
            > 
              {error === true && <p style={{color: "red"}}>Не верное имя пользователя или пароль!</p>}
              {/*Имя пользователя*/}
              <div style={{paddingTop:"30px"}}>
                <TextField 
                  error={error}
                  size="small"
                  label="Имя пользователя"
                  variant="outlined"
                  name="username"
                  id="username"
                  autoFocus={true}
                  autoComplete={true}           
                  value={username}
                  onChange={handleLoginChange}
                  className={classes.textField}
                />
              </div>
            </form>
            {/* Пароль */}
            <div align="center">
              <FormControl
                size="small"              
                variant="outlined"
                className={classes.formControl}
              >
                <InputLabel htmlFor="outlined-adornment-password" variant='outlined'> Пароль </InputLabel>
                <OutlinedInput
                  error={error}
                  labelWidth={60}
                  id="password"
                  name="password"
                  type={showPassword ? "text" : "password"}
                  value={password}
                  onChange={handlePasswordChange}
                  endAdornment={
                    <InputAdornment position="end">
                      <IconButton
                        aria-label="toggle password visibility"
                        onClick={handleClickShowPassword}
                        edge="end"
                        labelPlacement="start"
                      >
                        {showPassword ? <Visibility /> : <VisibilityOff />}
                      </IconButton>
                    </InputAdornment>
                  }                
                />              
              </FormControl>
            </div>
            {/*Checkbox + lableName */}
            <div align="center" style={{color:"#FFFFFF"}}>             
              <FormControl component="fieldset">
                <FormGroup aria-label="position" row>
                  <FormControlLabel                  
                    value="end"
                    control={
                      <Checkbox 
                        size="small" 
                        style={{color:"#FFFFFF"}} 
                        onChange={handleRememberMeChange}
                        checked={rememberMe}
                      />
                    }
                    label="Запомнить меня?"
                    labelPlacement="end"
                  />
                </FormGroup>
              </FormControl>
            </div>
            {/* BTN */}
            <div align="center" >
              <Button
                name="Login"
                variant="contained" 
                onClick={()=> LoginButtonClick()}
                className={classes.btn}             
              >
                <b>Вход</b>
              </Button>
            </div>
            </Card>
          </Typography>
        </Container>
      </React.Fragment>
  )
}
