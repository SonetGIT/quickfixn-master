import React, {useState, useEffect} from 'react';
import AppBar from "@material-ui/core/AppBar";
import Toolbar from "@material-ui/core/Toolbar";
import Typography from "@material-ui/core/Typography";
import { makeStyles} from '@material-ui/core/styles';
import logo from "./logo.png";

//Настройка заголовка
const useStyles = makeStyles((theme) => ({
  div: {
    flexGrow: 1,
    background:'#353535'
  },
  appBar: {
    background: '#b0bec5',
    backgroundImage: 'linear-gradient(180deg, #b0bec5 0%, #eceff1 150%)'
  },
  title: {
    flexGrow: 1,
    fontFamily: 'Sistem-ui',
    variant: 'body2',
    color:"#5a7b7b",
    align: 'center',
    paddingLeft: theme.spacing(1),
    textShadow:"2px 2px #eceff1"
  }
}));

export default function Header(props) {
  const classes = useStyles();  
  return (
    <div className={classes.div}>
      {/* background: "linear-gradient(to bottom, #b0bec5, #eceff1)" */}
      <AppBar position="static" className={classes.appBar}>
        <Toolbar>
          <img src={logo} alt="Logo"/>
          <Typography variant="h4" className={classes.title}>
            КФБ
          </Typography>
          <Typography variant="h4" className={classes.title}>
            Кыргызская Фондовая Биржа
          </Typography>
        </Toolbar>
      </AppBar>
    </div>
  );
}
