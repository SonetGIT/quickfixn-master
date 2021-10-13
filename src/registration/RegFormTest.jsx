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
import Email from "@material-ui/icons/Email";


const useStyles = makeStyles((theme) => ({
  root: {
    padding: theme.spacing(3),
  },
  buttonSpacing: {
    marginLeft: theme.spacing(1),
  },
}));

export default function Registration() {
  const classes = useStyles();
  const [email, setEmail] = useState(null)
  const [password, setPassword] = useState(null)
  const [emailDirty, setEmailDirty] = useState(false)
  const [passwordDirty, setPasswordDirty] = useState(false)
  const [emailError, setEmailError] = useState("Email не может быть пустым")
  const [passwordError, setPasswordError] = useState("пароль не может быть пустым")
  const [formValid, setFormValid] = useState(false)  
  
  useEffect(()=>{
    if(emailError || passwordError){
      setFormValid(false)
    }
    else { 
      setFormValid(true)
    }
    // console.log("AUTH", props)
    // let token = localStorage.getItem("token")
    // if(token){
    //   props.setToken(token)
    //   fetchUserProfile(token)
    //   console.log("TOKEN", token)
    // }
  },[])
  const emailHandler= (event) => {
    setEmail(event.target.value)
    const re = /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    if(!re.test(String(event.target.value).toLowerCase())){
      setEmailError("Некооректный email")
    } else {
      setEmailError("")
    }
  }
  const passHandler= (event) => {
    setPassword(event.target.value)
    if(event.target.value.length < 3 || event.target.value.length > 8){
      setPasswordError("Пароль должен быть длинее 3 и меньше 8")
      if(!event.target.value){
        setPasswordError("Пароль не может быть пустым")
      }
    }else {
      setPasswordError("")
    }
    
  }
  const blurHandler= (event) => {
    switch(event.target.name){
      case "email":
        setEmailDirty(true)
        break
      case "password":
        setPasswordDirty(true)  
        break
    }
  }

  return (
    <div>
      <form>
      <h1>Регистрация</h1>
      {(emailDirty && emailError) && 
        <div style={{color:"red"}}>{emailError}</div>}
      <input value={email} onBlur={blurHandler} onChange={emailHandler} name="email" type="text" placeholder="ffjj" />
      {(passwordDirty && passwordError) && 
        <div style={{color:"red"}}>{passwordError}</div>}
      <input value = {password} onBlur={blurHandler} onChange={passHandler}  name="password" type="password" placeholder="222" />
      <button disabled={!formValid} type="submit"> Регистрация </button>
      </form>
    </div>
  );
}
