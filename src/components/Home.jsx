import React, { useState, useEffect } from "react";
import clsx from 'clsx';
import { makeStyles } from '@material-ui/core/styles';
import { Grid } from '@material-ui/core';
import Drawer from '@material-ui/core/Drawer';
import List from '@material-ui/core/List';
import ListItem from '@material-ui/core/ListItem';
import ListItemIcon from '@material-ui/core/ListItemIcon';
import ListItemText from '@material-ui/core/ListItemText';
import AppBar from '@material-ui/core/AppBar';
import Toolbar from '@material-ui/core/Toolbar';
import Tooltip from '@material-ui/core/Tooltip';
import Typography from '@material-ui/core/Typography';
import Modal from '@material-ui/core/Modal';
import FormControl from '@material-ui/core/FormControl';
import FormControlLabel from '@material-ui/core/FormControlLabel';
import OutlinedInput from '@material-ui/core/OutlinedInput';
import InputLabel from '@material-ui/core/InputLabel';
import InputAdornment from '@material-ui/core/InputAdornment';
import IconButton from '@material-ui/core/IconButton';
import Button from '@material-ui/core/Button';
import Select from 'react-select';
import NumberFormat from 'react-number-format';
import PropTypes from 'prop-types';
import TextField from '@material-ui/core/TextField';
import Checkbox from '@material-ui/core/Checkbox';
import Paper from '@material-ui/core/Paper';

// Icons
import MainMenuIcon from '@material-ui/icons/HomeWork';
import MainReferenceIcon from '@material-ui/icons/FormatIndentIncrease';
import ConnectIcon from '@material-ui/icons/PowerSharp';
import ViewIcon from '@material-ui/icons/VisibilitySharp';
import ToolIcon from '@material-ui/icons/BuildSharp';
import ReportsIcon from '@material-ui/icons/ChromeReaderMode';
import SettingsIcon from '@material-ui/icons/Settings';
import WindowIcon from '@material-ui/icons/OpenInNew';
import HelpIcon from '@material-ui/icons/ContactSupport';
import AddIcon from '@material-ui/icons/AddBoxOutlined';
import RemoveIcon from '@material-ui/icons/IndeterminateCheckBoxOutlined';
//Иконки строки управления
import ConnectServerIcon from '@material-ui/icons/SettingsInputHdmiOutlined';
import RegUserIcon from '@material-ui/icons/PersonAdd'; //Регистрация пользователя
import ToolOutlinedIcon from '@material-ui/icons/PanToolOutlined';
import SendIcon from '@material-ui/icons/Email';
import LocalPrintshopIcon from '@material-ui/icons/LocalPrintshop';
import LibraryBooksIcon from '@material-ui/icons/LibraryBooks';
import BlockIcon from '@material-ui/icons/Block';
import CreditCardIcon from '@material-ui/icons/CreditCard'; //Финансовые инструменты
import AppOpenWinIcon from '@material-ui/icons/Description';
import TradeIcon from '@material-ui/icons/RecentActors'; //Сделка
import BayIcon from '@material-ui/icons/FormatBold'; //Продажа
import SaleIcon from '@material-ui/icons/MonetizationOn'; //Покупка
import AppAddIcon from '@material-ui/icons/PostAdd'; // Ввод прямой заявки
import REPOIcon from '@material-ui/icons/Compare'; //РЕПО сделка
import HowToRegIcon from '@material-ui/icons/HowToReg'; //Изменение цены оператором (метод фиксинга)
import OnlineTime  from "./OnlineTime"; //Онлайн время
import ExitToAppIcon from '@material-ui/icons/ExitToApp'; //Выход из системы
import RefreshFormIcon from '@material-ui/icons/Refresh';
// CUSTOM COMPONENTS
import ComponentManager from "./ComponentManager.jsx";
import Instruments from "./Instruments.jsx";
import OrderSell from "./OrderSell.jsx";
import OrderBuy from "./OrderBuy.jsx";
import ConfigurationFile from "../configuration/ConfigurationFile.json";

// Library
import { ToastContainer, toast } from 'react-toastify'; // https://fkhadra.github.io/react-toastify/introduction/
import 'react-toastify/dist/ReactToastify.css';
import { v4 as uuidv4 } from 'uuid';
var fetch = require('node-fetch');

//СТИЛИ
const useStyles = makeStyles((theme) => ({
  // Настройка Toolbar
  toolbar: {
    padding:"1px",
    border:"1px solid #78909c",
    minHeight: '1px',
    textAlign:"center",
    background: "linear-gradient(to bottom, #b0bec5, #eceff1)"
  },
  // Строка управления
  lineMenu: {
    margin:"3px",
    border:"1px inset #78909c",
    color:"#5a7b7b",
    fontSize:'23px',
    marginTop:"13px",    
    marginLeft:"9px" //расстояние между элементами
  },
  // Строка управления
  lineMenu1: {
    name:"insertForm",
    margin:"3px",
    border:"1px inset #78909c",
    color:"#5a7b7b",
    fontSize:'18px',
    marginTop:"13px",    
    marginLeft:"9px" //расстояние между элементами
  },
  // Иконка Выход
  exit: {
    margin:"3px",
    border:"1px inset #78909c",
    color:"#ff5b40",
    fontSize:'23px',
    marginTop:"13px",
    // marginRight:"10px",
    marginLeft:"15px"
  },
  // Стиль иконок выподающей меню
  listItemIcon: {    
    border:"1px inset #78909c",
    color:"#5a7b7b",
    fontSize:20,
  },
  // Стиль текста выподающей меню
  listItemText: {
    fontFamily: 'Courier',
    fontSize:16,
    variant: 'body2',
    color:"#455a64",
  }
}));
// ToolTip стили (подсказки)
const useStylesBootstrap = makeStyles((theme) => ({
  arrow: {
    color: theme.palette.common.white
  },
  tooltip: {
    backgroundColor: "#eceff1",
    border: "1px solid #78909c",
    fontFamily: 'Courier',
    color:"#5a7b7b",
    fontSize:13,
    textAlign:"center",
    fontWeight:"bold"  
  }
}));

function FloatFormat(props){
  const { inputRef, onChange, ...other } = props;
  return (
    <NumberFormat
      {...other}
      getInputRef={inputRef}
      onValueChange={values => {
        onChange({
          target: {
            value: values.value,
          },
        })
      }}
      decimalSeparator={"."}
      thousandSeparator={" "}
      isNumericString
    />
  )
}
FloatFormat.propTypes = {
  inputRef: PropTypes.func.isRequired,
  onChange: PropTypes.func.isRequired,
}
function IntegerFormat(props) {
  const { inputRef, onChange, ...other } = props;
  return (
    <NumberFormat
      {...other}
      getInputRef={inputRef}
      onValueChange={values => {
        onChange({
          target: {
            value: values.value,
          },
        });
      }}
      thousandSeparator={" "}
      isNumericString
    />
  );
}
IntegerFormat.propTypes = {
  inputRef: PropTypes.func.isRequired,
  onChange: PropTypes.func.isRequired,
}

//Home(props) - получаем переменные от родителя App.js 
export default function Home(props) {
  const classes = useStyles();
  const [state, setState] = useState({
    left: false,
    right: false,
  });
  const [task, setTask] = useState(null);
  const [taskVariables, setTaskVariables] = useState({});

  // SOCKET
  const [wsEndpoint] = useState(props.wsEndpoint) //Local
  const [kfbRESTApi] = useState(props.kfbRESTApi) //Local KFB main REST
  const [socket, setSocket] = useState(null)
  const [session_id, setSession_id] = useState(null)
  const [userProfile, setUserProfile] = useState(props.userProfile)
  const [instrumentTables, setInstrumentTables] = useState(["1"])
  const [updateState, setUpdateState] = useState(false)
  // ORDER
  const [showOrderSell, setShowOrderSell] = useState(false)
  const [showOrderBuy, setShowOrderBuy] = useState(false)
  
  // Main socket connection and data receiving
  if(socket === null){
    setSocket(new WebSocket(wsEndpoint))
    console.log("CONNECTING...", wsEndpoint)
  }
  if(socket !== null){
    socket.onmessage = async function(message){
      var incomingJson = JSON.parse(message.data)
      // console.log("Socket message", incomingJson)
      if(incomingJson.messageType === "session_id"){
        console.log("CONNECTED TO: ", wsEndpoint)
        console.log("NEW SESSION: ", incomingJson.session_id)
        setSession_id(incomingJson.session_id)
        socket.send(JSON.stringify({
          commandType: "setUserData",
          userId: "123123123123",// userProfile.userId,
          session_id: incomingJson.session_id,
          // userRole: userProfile.userRole,
          // partner: userProfile.partner,
          token: "Bearer " + "token"
        }))
      }
      else if(incomingJson.messageType === "userDataInserted"){
        console.log("userDataInserted", incomingJson.session_id)
      }
      else if(incomingJson.messageType === "distributions"){
        let unreadDistributions = JSON.parse(incomingJson.unreadDistributions)
        // console.log("unreadDistributions", unreadDistributions)
        if(unreadDistributions.length > 3){
          // setUnreadDistributions(unreadDistributions.substring(0, 3) + "+")
        }
        else{
          // setUnreadDistributions(unreadDistributions)
        }
      }
      else if(incomingJson.messageType === "toast"){
        console.log("TOAST", incomingJson)
        if(incomingJson.toastType === "success"){
          toast(incomingJson.toastText, {
            position: "top-right",
            autoClose: 4000,
            hideProgressBar: false,
            closeOnClick: true,
            pauseOnHover: true,
            draggable: true,
            progress: undefined,
          })
        }
        else if(incomingJson.toastType === "error"){
          toast.error(incomingJson.toastText, {
            position: "top-right",
            autoClose: 6000,
            hideProgressBar: false,
            closeOnClick: true,
            pauseOnHover: true,
            draggable: true,
            progress: undefined,
          })
        }
      }
    }
    socket.onclose = function(er){
      console.log("CONNECTION CLOSED ", wsEndpoint)
      // setSocket(null)
      setSocket(new WebSocket(wsEndpoint))
    }
  }

  function menuButtonClick(name, values) {
    console.log("BUTTON", name)
    if(name === "showMainRefSearchForm") {
      setTask(name);
      setTaskVariables({
        formDefId: "2aeadc9c-99f6-48fc-a2b3-a47c0670b109",
        buttons: "MainRefSearchBtn",
        gridFormDefId: "5fe6c5f6-ca17-4415-9d7d-57aed52cfad1",
        gridFormButtons: "MainRefGridBtn",
        tblFormBtns: "MainRefTblBtns",
        docListApi: "/api/Directory/Gets"
      })
    }
    else if(name === "showUserSearchForm"){
      setTask("showUserSearchForm")
      setTaskVariables({
        formDefId: "a0ea7b6a-c26a-4636-85fe-5b64c8b78cb2",
        buttons: "UserSearchBtn",
        gridFormDefId: "e5e74841-3d37-4835-8d15-9691d7902283",
        gridFormButtons: "UserGridBtn",
        tblFormBtns: "UserTblBtns",
        docListApi: "/api/Users/GetUsers"
      })
    }
    else if(name === "insertForm"){
      socket.send(JSON.stringify({commandType: "insertForm", session_id: session_id, token: "drgdrgdrh"}));
    }
    else{
      setTask(name)
    }
  }
  function setNewTask(inTask, inTaskVariables){
    setTask(inTask)
    setTaskVariables(inTaskVariables)
    console.log("NEW TASK: ", inTask, inTaskVariables)
    setUpdateState(getUUID())
  }

  // random UUID generator
  function getUUID(){
    return uuidv4()
  }
  async function getEnumData(Form){
    // console.log("COLLECT EN D", Form.sections)
    var enumData = []
    for (var section=0; section < Form.sections.length; section++){
      for(var item=0; item < Form.sections[section].contents.length; item++){
        if(Form.sections[section].contents[item].type === "Enum")
        {
          let enumName = Form.sections[section].contents[item].name
          let enumDef = Form.sections[section].contents[item].enumDef
          let apiName = ConfigurationFile.enumConfig[enumDef].apiName
          let newEnumList = await getEnumValues(apiName, enumName, enumDef)
          enumData.push(newEnumList)
        }
      }  
    }
    return enumData
  }
  // Request Enum Data from API
  async function getEnumValues(apiName, enumName, enumDef){
    // console.log("ENUM ITEM", apiName, enumName, enumDef)
    var newEnumValues = await fetch(
      kfbRESTApi + apiName,
      {
        "headers": { "content-type": "application/json" }
      }
    )
    .then(response => response.json())
    .then(function(res){
      // console.log("EN RESP", res)
      let parsedData = res.data
      let newEnumData = []
      let dataToCollect = ConfigurationFile.enumConfig[enumDef].data
      for(let key=0; key<parsedData.length; key++){
        let newItem = {}
        for(let item in dataToCollect){
          if(item === "id"){
            newItem[item] = parsedData[key][ConfigurationFile.enumConfig[enumDef].data[item]]
          }
          else{
            let fullLetter = null
            for(let n=0; n<ConfigurationFile.enumConfig[enumDef].data[item].length; n++){
              let itemToAppend = ConfigurationFile.enumConfig[enumDef].data[item][n]
              if(itemToAppend === "-" || itemToAppend === " "){
                fullLetter += itemToAppend
              }
              else{
                let newLetter = parsedData[key][itemToAppend]
                // console.log("NEW LETTER", newLetter)
                if(fullLetter === null){
                  fullLetter = newLetter
                }
                else{
                  fullLetter = fullLetter + parsedData[key][itemToAppend]
                }
              }
            }
            newItem[item] = fullLetter
          }
        }
        newEnumData.push(newItem)
      }
      var data = {
        name: enumName,
        data: newEnumData
      }
      // console.log("ENUM DATA: ", data)
      return data
    })
    .catch(function (error) {
      return console.log("Collecting enum data error: ", error)
    })
    // console.log("newEnumValues", newEnumValues)
    return newEnumValues
  }

  //Выход из ситемы
  function exitSystemClick(){
    localStorage.removeItem("token")
    props.setAuthenticated(false)
    props.setUserProfile({})
  }

  // ToolTip (подсказка) функция
  function BootstrapTooltip(props) {
    const clsToolTip = useStylesBootstrap();  
    return <Tooltip arrow classes={clsToolTip} {...props} />;
  }

  const toggleDrawer = (anchor, open) => (event) => {
    if (event.type === 'keydown' && (event.key === 'Tab' || event.key === 'Shift')) {
      return;
    }
    setState({ ...state, [anchor]: open });
  }

  //Объект внутри массива
  var menuArr = [
    {
        "name":"showMainRefSearchForm",
        "label":"Справочники",
        "icon":<MainReferenceIcon className={classes.listItemIcon}/>
    },
    {
      "name":"insertForm",
      "label":"Обновить формы",
      "icon":<RefreshFormIcon className={classes.listItemIcon}/>
    },
    {
        "name":"showConnectionForm",
        "label":"Соединение",
        "icon":<ConnectIcon className={classes.listItemIcon}/>
    }, 
    {
        "name":"showViewForm",
        "label":"Просмотр",
        "icon":<ViewIcon className={classes.listItemIcon}/>
    },
    {
      "name":"showToolForm",
      "label":"Инструмент",
      "icon":<ToolIcon className={classes.listItemIcon}/>
    },
    {
      "name":"showReportsForm",
      "label":"Отчеты",
      "icon":<ReportsIcon className={classes.listItemIcon}/>
    },
    {
      "name":"showSettingsForm",
      "label":"Настройки",
      "icon":<SettingsIcon className={classes.listItemIcon}/>
    },
    {
      "name":"showWindowForm",
      "label":"Окно",
      "icon":<WindowIcon className={classes.listItemIcon}/>
    },
    {
      "name":"showHelpForm",
      "label":"Помощь",
      "icon":<HelpIcon className={classes.listItemIcon}/>
    }
  ]

  const list = (anchor) => (
    <div
      onClick={toggleDrawer(anchor, false)}
      onKeyDown={toggleDrawer(anchor, false)}
    >
      <List>
        {menuArr.map((item, index) => (
          <ListItem  button key={index} onClick={() => menuButtonClick(item.name)}>
            <ListItemIcon >{item.icon}</ListItemIcon>
            <ListItemText>
              <Typography className={classes.listItemText}>
              {item.label}
              </Typography>
              </ListItemText>
          </ListItem>
        ))}
      </List>      
    </div>
  )
  // ОТРИСОВКА
  return (
    <Grid
      container
      direction="column"
      justifyContent="center"
      alignItems="center"
    >
      <Grid item xs={12}>
        <Grid
          container
          direction="row"
          justifyContent="center"
          alignItems="center"
        >
          <Grid item xs={12}>
            <AppBar position="fixed">
              <Toolbar className={classes.toolbar}>
                <Grid
                  container
                  direction="row"
                  justifyContent="space-between"
                  alignItems="flex-start"
                >
                  <Grid itemxs={8}>
                    <React.Fragment>
                      <MainMenuIcon className={classes.lineMenu} onClick={toggleDrawer("menu", true)}></MainMenuIcon>
                      {/* выподающее меню */}
                      <Drawer open={state["menu"]} onClose={toggleDrawer("menu", false)}>
                        {list("menu")}
                      </Drawer>
                    </React.Fragment>
                
                    {/* СТРОКА УПРАВЛЕНИЯ */}          
                    <BootstrapTooltip title="Подключения к Серверу" placement="top">
                      <ConnectServerIcon className={classes.lineMenu} />
                    </BootstrapTooltip>
                    <BootstrapTooltip title="Рег-я поль-я для входа в ТС">
                      <RegUserIcon className={classes.lineMenu} onClick={()=> menuButtonClick("showUserSearchForm")} />              
                    </BootstrapTooltip>
                    <BootstrapTooltip title="Выбор рабочего набора инструментов">
                      <ToolOutlinedIcon className={classes.lineMenu} />
                    </BootstrapTooltip>
                    <BootstrapTooltip title="Отправка сообщения другим пользователям">
                      <SendIcon className={classes.lineMenu} />
                    </BootstrapTooltip>
                    <BootstrapTooltip title="Получение документов с Сервера">
                      <LibraryBooksIcon className={classes.lineMenu}/>
                    </BootstrapTooltip>
                    <BootstrapTooltip title="Просмотр и печать полученных документов с Сервера">
                      <LocalPrintshopIcon className={classes.lineMenu}/>
                    </BootstrapTooltip>
                    <BootstrapTooltip title="Блокирование доступа в рабочее место участника торгов">
                      <BlockIcon className={classes.lineMenu}/>
                    </BootstrapTooltip>
                    <BootstrapTooltip title="Открытие окна «Финансовые инструменты»">
                      <CreditCardIcon className={classes.lineMenu}/>
                    </BootstrapTooltip>
                    <BootstrapTooltip title="Открытие окна «Заявки»">
                      <AppOpenWinIcon className={classes.lineMenu}/>
                    </BootstrapTooltip>
                    <BootstrapTooltip title="Открытие окна «Сделки»">
                      <TradeIcon className={classes.lineMenu}/>
                    </BootstrapTooltip>
                    <BootstrapTooltip title="Ввод прямой заявки">
                      <AppAddIcon className={classes.lineMenu}/>
                    </BootstrapTooltip>
                    <BootstrapTooltip title="Ввод РЕПО заявки методом прямых сделок">
                      <REPOIcon className={classes.lineMenu}/>
                    </BootstrapTooltip>
                    <BootstrapTooltip title="Изменение цены оператором на один шаг при торговле методом фиксинга">
                      <HowToRegIcon className={classes.lineMenu}/>
                    </BootstrapTooltip>
                    <BootstrapTooltip title="Ввод заявки на ПОКУПКУ методом открытых торгов">
                      <BayIcon 
                        className={classes.lineMenu}
                        style={{color: "green", borderBlockColor: "red"}}
                        onClick={()=> setShowOrderBuy(!showOrderBuy)}
                        />
                    </BootstrapTooltip>
                    <BootstrapTooltip title="Ввод заявки на ПРОДАЖУ методом открытых торгов;">
                      <SaleIcon 
                        className={classes.lineMenu} 
                        style={{color: "green", borderBlockColor: "red"}} 
                        onClick={()=> setShowOrderSell(!showOrderSell)}
                      />
                    </BootstrapTooltip>
                  </Grid>
                  <Grid itemxs={3}>
                    {/* ОНЛАЙН ВРЕМЯ */}
                    <div  style={{marginTop:"15px"}}>
                      <OnlineTime />
                    </div>
                  </Grid>
                  <Grid itemxs={1}>
                    <BootstrapTooltip title="Выход из системы">
                      <ExitToAppIcon className={classes.exit} onClick={()=> exitSystemClick()}/>
                    </BootstrapTooltip>  
                  </Grid>
                </Grid>   
              </Toolbar>        
            </AppBar>
          </Grid>
        </Grid>
      </Grid>
      {task !== null ?
        <Grid style={{paddingTop: "100px", paddingLeft: "200px"}}>
          <ComponentManager
            // VARS
            id={task}
            key={task}
            task={task}
            setNewTask={setNewTask}
            taskVariables={taskVariables}
            kfbRESTApi={kfbRESTApi}
            userProfile={userProfile}
            // FUNCTIONS
            getEnumData={getEnumData}
          />
        </Grid>
        :
        instrumentTables.map(instrumentTable =>
          <Grid style={{paddingTop: "50px"}}>
            <Instruments
              kfbRESTApi={kfbRESTApi}
              getEnumData={getEnumData}
            />
          </Grid>
        )
      }
      {showOrderSell === true &&
        <OrderSell 
          kfbRESTApi={kfbRESTApi}
          setShowOrderSell={setShowOrderSell}
        />
      }
      {showOrderBuy === true &&
        <OrderBuy
        kfbRESTApi={kfbRESTApi}
        setShowOrderBuy={setShowOrderBuy}
        />
      }
      {/* Вызов toast */}
      <ToastContainer/>
    </Grid>
  );
}