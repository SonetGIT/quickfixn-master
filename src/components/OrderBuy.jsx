import React, { useState, useEffect } from "react";
import { makeStyles } from '@material-ui/core/styles';
import Button from '@material-ui/core/Button';
import Select from 'react-select';
import NumberFormat from 'react-number-format';
import PropTypes from 'prop-types';
import TextField from '@material-ui/core/TextField';
import Tab from '@mui/material/Tab';
import TextareaAutosize from '@mui/material/TextareaAutosize';

import Checkbox from '@material-ui/core/Checkbox';
// Icons
import IconButton from '@material-ui/core/IconButton';
import AddIcon from '@mui/icons-material/AddCircleOutline';
import RemoveIcon from '@mui/icons-material/RemoveCircleOutline';
import CloseIcon from '@material-ui/icons/Close';
import DeleteForeverIcon from '@material-ui/icons/DeleteForever';
// CUSTOM COMPONENTS
import ConfigurationFile from "../configuration/ConfigurationFile.json";
// Library
import 'react-toastify/dist/ReactToastify.css';
import { v4 as uuidv4 } from 'uuid';
import Draggable from 'react-draggable';
import { red } from "@material-ui/core/colors";
import { padding } from "@mui/system";
var fetch = require('node-fetch');

const useStyles = makeStyles((theme) => ({
  modal: {
    position: 'absolute',
    width: 550,
    // height: 200,
    backgroundColor:"#eceff1", // theme.palette.background.paper,
    borderRadius:"10px",
    border: '1px solid #78909c',
    boxShadow: theme.shadows[1],
    padding: 7 
    // theme.spacing(1, 1, 1),
  }
}))
function getModalStyle() {
  const top = 15;
  const left = 25;
  return {
    top: `${top}%`,
    left: `${left}%`,
    // transform: `translate(-${top}%, -${left}%)`,
  };
}
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
            name: props.name,
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
  name: PropTypes.string.isRequired,
  onChange: PropTypes.func.isRequired,
}

//Home(props) - получаем переменные от родителя App.js 
export default function Home(props) {
  const classes = useStyles()
  const [kfbRESTApi] = useState(props.kfbRESTApi) //Local KFB main REST
  const [userProfile, setUserProfile] = useState(props.userProfile)
  const [updateState, setUpdateState] = useState(false)
  const [enumData, setEnumData] = useState({})
  const [enumOptions, setEnumOptions] = useState({})
  const [selectedOptions, setSelectedOptions] = useState({})
  const [modalStyle] = useState(getModalStyle)
  const [orderType, setOrderType] = useState("limited")  
  // FIELDS
  const [lots, setLots] = useState([])
  const [fieldValue, setFieldValue] = useState({
    tradingAccount: null,
    amountOfLots: 0,
    amountOfLotsStep: 1,
    priceForInstrument: 100,
    priceForInstrumentStep: 1,
    checkBox:true
  })  

  useEffect(async ()=>{
    console.log("ORDER SELL PROPS", props)
    let enumDataToCollect = [
      {enumName: "financeInstruments", enumDef: "3e819d7e-25d0-4a04-a3ff-092fd348a375"},
      {enumName: "tradingAccount", enumDef: "c324d86f-3a3b-43b2-9514-d983b2982794"},
    ]
    // await getEnumValues("financeInstruments", "3e819d7e-25d0-4a04-a3ff-092fd348a375")
    // await getEnumValues("tradingAccount", "c324d86f-3a3b-43b2-9514-d983b2982794")
    await getEnumValues(enumDataToCollect)
    // setUpdateState(getUUID())
  },[])

  // random UUID generator
  function getUUID(){
    return uuidv4()
  }
  // Request Enum Data from API
  async function getEnumValues(list){
    for(let i=0; i<list.length; i++){
      let enumName = list[i].enumName
      let enumDef = list[i].enumDef
      console.log("ENUM ITEM", enumName, enumDef)
      await fetch(
        kfbRESTApi + ConfigurationFile.enumConfig[enumDef].apiName,
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
        
        // let item = {
        //   name: enumName,
        //   data: newEnumData
        // }
        createEnumOptions(enumName, newEnumData)
        // var data = enumData
        // data.push(item)
        let nd = enumData
        nd[enumName] = newEnumData
        setEnumData(enumData)
        // setEnumData({...enumData, [enumName]: newEnumData})
        console.log("ENUM DATA: ", enumData)
        // setUpdateState(getUUID())
      })
      .catch(function (error) {
        return console.log("Collecting enum data error: ", error)
      })
      // console.log("newEnumValues", newEnumValues)
    }
    
  }
  function createEnumOptions(name, data){
    console.log("ENUMS", name, data)
    let options = [{
      "value": "",
      "label": "Пусто",
      "name" : name
    }]
    for(let d=0; d<data.length; d++){
        options.push({
        "value": data[d].id,
        "label": data[d].label,
        "name" : name
      })
    }
    let nd = enumOptions
    nd[name] = options
    setEnumOptions(nd)
    // setEnumOptions({...enumOptions, [name]: options})
    console.log("OPTIONS", enumOptions)
    setUpdateState(getUUID())
  }
  // INPUT HANDLERS
  // SELECT
  function handleSelectChange(option){
    setSelectedOptions({...selectedOptions, [option.name]: option})
    setFieldValue({...fieldValue, [option.name]: option.value})
    console.log("OPT", option)
  }
  // INT
  const handleIntChange = (event) => {
    console.log("EVENT", event.target.name, event.target.value)
    let stringNum = event.target.value.toString().replace(/ /g,'')
    let int = parseInt(stringNum)
    setFieldValue({ ...fieldValue, [event.target.name]: int })
  }
  function addLots(){
    // console.log("LOTS AMOUNT", fieldValue["amountOfLots"], fieldValue["amountOfLotsStep"])
    let newAmount = fieldValue["amountOfLots"] + fieldValue["amountOfLotsStep"]
    // console.log("NEW AMOUNT", newAmount)
    setFieldValue({ ...fieldValue, ["amountOfLots"]: newAmount})
    let newLots = lots
    newLots.push({
      "price": fieldValue["priceForInstrument"],
      "amount": fieldValue["amountOfLotsStep"],
      "profit": 0,
      "volume": 0
    })
    // var removedItem = fruits.splice(pos, 1); // так можно удалить элемент
    // var last = fruits.pop(); // удалим Апельсин (из конца)
    // var first = fruits.shift(); // удалим Яблоко (из начала)
    // var newLength = fruits.unshift('Клубника') // добавляет в начало
  }
  function deleteLot(i){
    // console.log("DELETE LOT", i, lots[i])
    let newLots = lots.slice()
    newLots.splice(i, 1) // delete element
    setLots(newLots)
    setFieldValue({ ...fieldValue, ["amountOfLots"]: newLots.length})
  }
  function addPriceForInstrument(){
    let newPrice = fieldValue["priceForInstrument"] + fieldValue["priceForInstrumentStep"]
    setFieldValue({ ...fieldValue, ["priceForInstrument"]: newPrice})
    // console.log("NEW PRICE", newPrice)
  }
  function reducePriceForInstrument(){
    let newPrice = fieldValue["priceForInstrument"] - fieldValue["priceForInstrumentStep"]
    setFieldValue({ ...fieldValue, ["priceForInstrument"]: newPrice})
    // console.log("NEW PRICE", newPrice)
  }
  function agreeForm(f) {
    // Если поставлен флажок, снимаем блокирование кнопки
    if (newPrice.checked) newPrice.submit.disabled = 0 
    // В противном случае вновь блокируем кнопку
    else newPrice.submit.disabled = 1
   }
  // #b0bec5, #eceff1
  return (
    <Draggable handle="div">
      <div style={modalStyle} className={classes.modal}>
       <table align="center" style={{border: "1px solid #78909c", marginTop:2, marginBottom:5}}> 
       <tr>      
        <td align="center" width="99%" style={{lineHeight: "2px", fontSize: "15px", fontFamily: "Courier New"}}><h4>Ввод заявки на ПОКУПКУ методом открытых торгов</h4></td>
        <td style={{lineHeight: "2px"}} onClick={()=> props.setShowOrderSell(false)}><IconButton><CloseIcon/></IconButton></td>
       </tr>
      </table> 
      <div style={{border:"1px ridge #78909c", backgroundColor:"#dee4e7", marginTop:2}}>        
        <div>
        <table align="center" width="100%">
          <tr>
            <td width="2%" style={{fontSize: "15px", fontFamily: "Courier New", fontWeight:"bold"}}>Торговый счёт</td>
            <td width="20%" height="10%">
              <Select                
                name = {"tradingAccount"}
                placeholder = {"Выбрать..."}
                value = {selectedOptions["tradingAccount"]}
                onChange = {handleSelectChange}
                options = {enumOptions["tradingAccount"]}                
              />
            </td>
            <td width="2%" style={{fontSize: "15px", fontFamily: "Courier New", fontWeight:"bold"}}>Код клиента</td>
            <td width="20%" height="10%">
              <Select
                name = {"clientCode"}
                placeholder = {"Выбрать..."}
                value = {selectedOptions["clientCode"]}
                onChange = {handleSelectChange}
                options = {enumOptions["clientCode"]}
              />
            </td>
          </tr>          
        </table>
        <table width="100%">
          <tr>
            <td>
              <Select
                name = {"financeInstruments"}
                placeholder = {"Найти инструмент..."}
                value = {selectedOptions["financeInstruments"]}
                onChange = {handleSelectChange}
                options = {enumOptions["financeInstruments"]}
              />
            </td>
          </tr>
        </table>        
        <table align="center" width="100%">
          <tr>
            <td style={{fontSize: "15px", fontFamily: "Courier New", fontWeight:"bold", width:145}}>Количество лотов</td>
            <td style={{height:20, width:175}}>
              <TextareaAutosize
                size = "small"
                style={{width: 120}}
                variant = "outlined"
                value = {fieldValue.amountOfLots}
                onChange = {handleIntChange}
                name = "amountOfLots"
                // defaultValue = {fieldValue["amountOfLots"]}
                disabled={true}
                InputProps={{inputComponent: IntegerFormat}}
              />              
              <AddIcon fontSize="small" style={{paddingTop: 3, paddingLeft:3}} onClick={()=> addLots()}/>
              <RemoveIcon fontSize="small" style={{paddingTop: 3}} onClick={()=> deleteLot()}/>
            </td>
            <td style={{fontSize: "15px", fontFamily: "Courier New", textDecoration:"underline"}}>Максимум:</td>            
          </tr>
          <tr>
            <td></td>
            <td style={{fontSize: "12px", fontFamily: "Courier New", paddingLeft:15}}>Размер лота: {fieldValue["priceForInstrumentStep"]}</td>
          </tr>
        </table>
        <table align="center" width="100%">
          <tr>
            <td align="center">
             <Button
                class="marketLimitBtn"
                variant="outlined"                
                onClick = {() => setOrderType("limited")}
                >Лимитная
             </Button>  
             <Button
                class="marketLimitBtn"
                variant="outlined"                   
                onClick = {() => setOrderType("market")}                
                >Рыночная
              </Button>                          
            </td>
          </tr>
        </table>
        {/* ЛИМИТНАЯ */}
        {orderType === "limited" &&
          <div overflow="auto">
            <table align="center" width="100%">
              <tr>
                <td style={{fontSize: "15px", fontFamily: "Courier New", fontWeight:"bold", width:165}}>Цена за инструмент</td>
                <td style={{height: 20, width:175}}>
                  <TextareaAutosize
                    style={{width: 120}}
                    size="small"
                    variant="outlined"
                    value = {fieldValue.priceForInstrument}                    
                    onChange = {handleIntChange}
                    name = "priceForInstrument"
                    // onBlur = {handleIntChange}
                    // name = {contentItem.name}
                    InputProps={{inputComponent: IntegerFormat}}
                  />
                  <AddIcon fontSize="small" style={{paddingTop: 3, paddingLeft:3}} onClick={()=> addPriceForInstrument()}/>
                  <RemoveIcon fontSize="small" style={{paddingTop: 3}} onClick={()=> reducePriceForInstrument()}/>                  
                </td>
                <td style={{fontSize: "15px", fontFamily: "Courier New"}}>Сумма:</td>
              </tr>
              <tr>
                <td></td>
                <td style={{fontSize: "12px", fontFamily: "Courier New", paddingLeft:25}}>шаг цены: {fieldValue["priceForInstrumentStep"]}</td>
              </tr>
            </table>
            <div overflow="auto">
              <table align="center" width="100%">
                <tr>
                  <td style={{fontSize: "15px", fontFamily: "Courier New", fontWeight:"bold"}}>Цены:</td>
                </tr>
              </table>
              <table align="center" width="60%">
                <tr>
                  <td style={{fontSize: "15px", fontFamily: "Courier New"}}>Покупка</td>
                  <td style={{fontSize: "15px", fontFamily: "Courier New"}}>Продажа</td>
                  <td style={{fontSize: "15px", fontFamily: "Courier New"}}>Последняя</td>
                </tr>
              </table>
              <table>
              <tr>              
                <td style={{fontSize: "15px", fontFamily: "Courier New", fontWeight:"bold"}}>Связанная стоп-заявка</td>
                <Checkbox 
                  size="small" 
                  style={{color:"green"}}
                  name="checkbox"
                  onClick={()=> agreeForm()}
                />
              </tr>
              </table>
              <table align="center" width="100%">
              <tr>
                <td style={{fontSize: "15px", fontFamily: "Courier New", fontWeight:"bold", width:140}}>Если цена больше или равна</td>
                <td style={{height: 20, width:175}}>
                  <TextareaAutosize
                    style={{width: 120}}
                    size="small"
                    variant="outlined"
                    value = {fieldValue.priceForInstrument}
                    onChange = {handleIntChange}                    
                    name = "priceForInstrument"      
                    // onBlur = {handleIntChange}
                    // name = {contentItem.name}
                    InputProps={{inputComponent: IntegerFormat}}
                  />
                  <AddIcon fontSize="small" style={{paddingTop: 3, paddingLeft:3}} onClick={()=> addPriceForInstrument()}/>
                  <RemoveIcon fontSize="small" style={{paddingTop: 3}} onClick={()=> reducePriceForInstrument()}/>                  
                </td>                
              </tr>
              <tr>
                <td></td>
                <td style={{fontSize: "12px", fontFamily: "Courier New", paddingLeft:25}}>шаг цены: {fieldValue["priceForInstrumentStep"]}</td>
              </tr>
             </table>
             <table align="center" width="100%">
              <tr>
                <td style={{fontSize: "15px", fontFamily: "Courier New", fontWeight:"bold", width:220}}>Выставить заявку по цене</td>
                <td style={{height: 20, width:175}}>
                  <TextareaAutosize
                    style={{width: 120}}
                    size="small"
                    variant="outlined"
                    value = {fieldValue.priceForInstrument}
                    onChange = {handleIntChange}
                    name = "priceForInstrument"
                    // onBlur = {handleIntChange}
                    // name = {contentItem.name}
                    InputProps={{inputComponent: IntegerFormat}}
                  />
                  <AddIcon fontSize="small" style={{paddingTop: 3, paddingLeft:3}} onClick={()=> addPriceForInstrument()}/>
                  <RemoveIcon fontSize="small" style={{paddingTop: 3}} onClick={()=> reducePriceForInstrument()}/>                  
                </td>
                <td style={{fontSize: "15px", fontFamily: "Courier New"}}>Сумма:</td>
              </tr>
              <tr>
                <td></td>
                <td style={{fontSize: "12px", fontFamily: "Courier New", paddingLeft:25}}>шаг цены: {fieldValue["priceForInstrumentStep"]}</td>
              </tr>
             </table>
            </div>            
          </div>
        }
        {/* РЫНОЧНАЯ */}
        {orderType === "market" &&
          <div overflow="auto">
            <table align="center" width="100%">
              <tr>
                <td style={{fontSize: "15px", fontFamily: "Courier New", fontWeight:"bold"}}>Цена за инструмент: Рыночная</td>
              </tr>
              <tr>
                <td style={{fontSize: "15px", fontFamily: "Courier New", fontWeight:"bold", paddingTop:7, paddingBottom:7}}>Сумма:</td>
              </tr>
              <tr>
                <td style={{fontSize: "15px", fontFamily: "Courier New", fontWeight:"bold"}}>Цены:</td>
              </tr>
            </table>
            <table align="center" width="60%">
              <tr>
                <td style={{fontSize: "15px", fontFamily: "Courier New"}}>Покупка</td>
                <td style={{fontSize: "15px", fontFamily: "Courier New"}}>Продажа</td>
                <td style={{fontSize: "15px", fontFamily: "Courier New"}}>Последняя</td>
              </tr>
            </table>
          </div>
        }        
        <table align="center" width="100%" style={{paddingTop:5}}>
          <tr>
            <td align="right">
              <Button
               class="orderCloseBtn"
               variant="contained"
                >Отправить заявку
              </Button>
              <Button
                  class="orderCloseBtn"
                  variant="contained"                         
                  onClick = {()=> props.setShowOrderSell(false)}                  
                >Отмена
              </Button>
            </td>
          </tr>
         </table>
         </div>
        </div>    
      </div>
    </Draggable>
  )
}