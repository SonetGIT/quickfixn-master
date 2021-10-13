import React, {useState, useEffect} from 'react';
import { makeStyles } from '@material-ui/core/styles';
import Table from "@material-ui/core/Table"; // Material ui table for usual form
import TableHead from "@material-ui/core/TableHead";
import TableRow from "@material-ui/core/TableRow";
import TableBody from "@material-ui/core/TableBody";
import TableCell from "@material-ui/core/TableCell";
import Button from '@material-ui/core/Button';
import { Grid } from '@material-ui/core';
import Paper from '@material-ui/core/Paper';
import GridSelect from '@material-ui/core/Select';
import FormControl from '@material-ui/core/FormControl';
import IconButton from '@material-ui/core/IconButton';
import MenuItem from '@material-ui/core/MenuItem';
import LinearProgress from '@material-ui/core/LinearProgress';
import Snackbar from '@material-ui/core/Snackbar';
import NumberFormat from 'react-number-format';
import PropTypes from 'prop-types';
import Tooltip from '@material-ui/core/Tooltip';
// Icons
import FirstPageIcon from '@material-ui/icons/FirstPage';
import ArrowForwardIosIcon from '@material-ui/icons/ArrowForwardIos';
import ArrowBackIosIcon from '@material-ui/icons/ArrowBackIos';
import ArrowDropDownIcon from '@material-ui/icons/ArrowDropDown';
import ArrowDropUpIcon from '@material-ui/icons/ArrowDropUp';
import ReplayIcon from '@material-ui/icons/Replay';
// Form components
import TextField from '@material-ui/core/TextField';
import Select from 'react-select'; // https://react-select.com/home
import Checkbox from '@material-ui/core/Checkbox';

// Custom components
// import "../../components/GridForm/GridFormCSS.css"
import Buttons from "../configuration/Buttons.json";
import GridFormButtons from '../configuration/GridFormButtons.json';
import TblFormBtns from '../configuration/TableFormButtons.json'
// Libraries
import swal from 'sweetalert'; // https://sweetalert.js.org/guides/
import hotkeys from 'hotkeys-js'; // https://github.com/jaywcjlove/hotkeys
import uuid from "uuid";
//Style
import "../styles/generalStyles.css"

var fetch = require('node-fetch');
var generator = require('generate-password');

export default (props) => {  
    // This.state
  const [kfbRESTApi] = useState(props.kfbRESTApi)
  const [taskVariables] = useState(props.taskVariables);
  const [userProfile] = useState(props.userProfile)
  const [Form, setForm] = useState(null)
  const [buttons, setButtons] = useState([])
  const [gridForm, setGridForm] = useState(null)
  const [gridFormButtons, setGridFormButtons] = useState([])
  const [tblFormBtns, setTblFromBtns] = useState([])
  const [selectedDoc, setSelectedDoc] = useState(null)
  // const [docId, setDocId] = useState(props.userTask.docId)
  const [formType] = useState("edit")
  const [docList, setDocList] = useState(null)
  const [filteredDocList, setFilteredDocList] = useState(null)
  const [initialDocList, setInitialDocList] = useState(null)  
  const [enumData, setEnumData] = useState([])
  const [enumOptions, setEnumOptions] = useState({})
  const [gridFormEnumData, setGridFormEnumData] = useState([])
  const [fieldValue, setFieldValue] = useState({})
  const [selectedOptions, setSelectedOptions] = useState([])
  
  const [page, setPage] = useState(1)
  const [size, setSize] = useState(10)
  const [showSnackBar, setShowSnackBar] = useState(false)
  const [snackBarMessage, setSnackBarMessage] = useState("")
  // const [sectionColor] = useState("#B2E0C9")
  const [updateState, setUpdateState] = useState(false)
  const [task] = useState(props.task)
  const [sortedColumn, setSortedColumn] = useState("null")
  const [sortedColumnOrder, setSortedColumnOrder] = useState(1)

  // Set data from props to state of component
  useEffect(()=>{
    console.log("INSTRUMENTS PROPS", props)
    // if(taskVariables.gridFormDefId !== null){
      fetchGridFrom("69a31ec9-fef1-43f8-ac92-4e490670a509")
      fetchDocList("/api/FinanceInstruments")
    // }
    setUpdateState(getUUID())
  },[])

  async function fetchDocList(docListApi){
    console.log("API", kfbRESTApi + docListApi)
    await fetch(kfbRESTApi + docListApi, 
      {
        "headers": { "content-type": "application/json" }
      }
    )
    .then(response => response.json())
    .then(function(res){
      console.log("DocList", res)
      // setFilteredDocList(res.data)
      // setInitialDocList(res.data)
      let sortedDocList = res.data.sort(dynamicSort("id", 1, "Int"))
      setDocList(sortedDocList)
      // fetchBySize(0, 9, sortedDocList)         
    })
  }
  async function fetchGridFrom(defid){
    await fetch(kfbRESTApi + "/api/Metadata/GetByDefId?defid=" + defid,
      {
        "headers": { "content-type": "application/json" }
      }
    )
    .then(response => response.json())
    .then(async function(res){
      let f = JSON.parse(res.data.data)
      setGridForm(f)
      let enums = await props.getEnumData(f)
      console.log("ENUMDATA", enums)
      setGridFormEnumData(enums)
      // createEnumOptions(enums)
      console.log("GFORM", f)
    })
  }
  // Create appropriate structure for enum options
  function createEnumOptions(enums){
    let newEnumOptions = {}
    for(let i=0; i<enums.length; i++){
      let options = [{
        "value": "",
        "label": "Пусто",
        "name" : enums[i].name
      }]
      for(let d=0; d<enums[i].data.length; d++){
          options.push({
          "value": enums[i].data[d].id,
          "label": enums[i].data[d].label,
          "name" : enums[i].name
        })
      }
      newEnumOptions[enums[i].name] = options
    }
    setEnumOptions(newEnumOptions)
  }
  
  function getContentType(key){
    for(let s=0; s<Form.sections.length; s++){
      for(let c=0; c<Form.sections[s].contents.length; c++){
        if(key === Form.sections[s].contents[c].name){
          return Form.sections[s].contents[c].type
        }
      }
    }
  }
  // filter docList by filled parameters
  function filterDocList(fetchFrom, fetchTo, Data, fields){
    var newDocList = []
    if(Object.keys(fields).length === 0){
      newDocList = Data
    }
    else{
      for(let i=0; i<Data.length; i++){
        let match = false
        for(let key in fields){
          if(fields[key] === null || fields[key] === "" || fields[key] === undefined){
            match = true
          }
          else{
            if(Data[i][key] !== null){
              let contentType = getContentType(key)
              if(contentType === "Text"){
                try{
                  let searchField = fields[key].toLowerCase()
                  let dataField = Data[i][key].toLowerCase()
                  let includeSearch = dataField.includes(searchField)
                  if(includeSearch === true){
                    match = true
                  }
                  else{
                    match = false
                    break
                  }
                }
                catch(er){
                  console.log("ERR", er)
                  match = true
                }
              }
              else if(contentType === "Int" || contentType === "Float"){
                if(fields[key] >= 0 || fields[key] < 0){
                  let searchField = fields[key].toString()
                  let dataField = Data[i][key].toString()
                  let includeSearch = dataField.includes(searchField)
                  if(includeSearch === true){
                    match = true
                  }
                  else{
                    match = false
                    break
                  }
                }
                else{match = true}
              }
              else if(contentType === "Enum"){
                
                if(fields[key] === Data[i][key]){
                  // console.log("ENUMS", key, fields[key], Data[i][key])
                  match = true
                }
                else{
                  match = false
                  break
                }
              }
              else if(contentType === "DateTime"){
                let searchField = props.parseDate(fields[key])
                let dataField = props.parseDate(Data[i][key])
                // console.log("DATES", searchField, dataField, searchField.includes(dataField))
                if(searchField === "NaN-NaN-NaN"){
                  match = true
                }
                else{
                  if(searchField.includes(dataField) === true){
                    match = true
                  }
                  else{
                    match = false
                    break
                  }
                }
              }
              else if(contentType === "Bool"){
                if(fields[key] === Data[i][key]){
                  match = true
                }
                else{
                  match = false
                  break
                }
              }
            }
            else{
              match = false
              break
            }
          }
        }
        if(match === true){
          newDocList.push(Data[i])
        }
      }
    }
    setFilteredDocList(newDocList)
    fetchBySize(fetchFrom, fetchTo, newDocList)
  }
  // get rows amount of filtered docList by size
  function fetchBySize(fetchFrom, fetchTo, Data){
    // console.log("fetchFrom", "fetchTo")
    let newDocList = []
    for(let i=fetchFrom; i<=fetchTo; i++){
      if(Data[i] !== undefined){
        newDocList.push(Data[i])
      }
    }
    // setFilteredDocList(Data)
    setDocList(newDocList)
  }
  function handleCloseSnackBar(){
    setShowSnackBar(false)
  }
  // random UUID generator
  function getUUID() {
    const uuid = require("uuid")
    return uuid.v1()
  }
  // random numbers generator
  function keyGen(length){
    var password = generator.generate({
      length: length,
      numbers: true
    })
    return password
  }
  function swAllert(text, icon){
    return(
      swal({
        text: text,
        icon: icon,
        buttons: {ok: "Ок"},
      })
    )
  }
  function buttonClick(buttonName, item){
    console.log("ITEM", item)
    if(buttonName === "findDocument"){
      // console.log("findDocument")
      filterDocList(0, size-1, initialDocList, fieldValue)
    }
    //Открыть форму создания пользователя
    else if(buttonName === "showCreateUserForm"){               
      let taskVars = {
        formDefId: "a25af3d7-b504-488a-8bfb-a8d9cf0594f0",
        buttons:"UserSaveBtn",
        gridFormDefId: null,
        gridFormButtons: null,
        tblFormBtns: null,
        docListApi:null,
        selectedDoc: item
      }      
      props.setNewTask("showCreateUserForm", taskVars)
      console.log("button showCreateUserForm: ", taskVars)      
    }
    else if (buttonName === "close"){
      props.setNewTask(null, {})
      console.log("button close: ")
    }
    else if (buttonName === "cancel"){
      props.setNewTask(null, {})
      console.log("button cancel: ")
    }
    else if(buttonName === "cancel"){
      const commandJson = 
      {
        userRole: userProfile.userRole,
        variables: {
          authorization: {value: "token"},
          userAction: {value: "cancel"},
        }
      }
      console.log("button cancel: ", commandJson)
      // props.sendFieldValues(commandJson)
      // props.clearTabData(process_id)
    }
    else{
      console.log("UNKNOWN Button ", buttonName)
    }
  }

  // Create grid form components
  function getGridFormItems(dataItem, contentItem){
    let value = dataItem[contentItem.name]
    let name = contentItem.name
    let type = contentItem.type
    if(type === "Enum"){
      if(value === null || value === "" || value === undefined){
        return(<td align="center" style={{color: "black", fontSize: 12}}>-</td>)
      }
      else{
        for(let i=0; i<gridFormEnumData.length; i++){
          if(name === gridFormEnumData[i].name){
            for(let l=0; l<gridFormEnumData[i].data.length; l++){
              if(gridFormEnumData[i].data[l].id === parseInt(value)){
                return gridFormEnumData[i].data[l].label
              }
            }
          }
        }
      }
    }
    else if(type === "Bool"){
      // console.log("ITEM", name, value, type)
      return(
        <Checkbox
          style={{maxWidth: 20, height: 15, color: value === false ? "red" : "green"}}
          checked = {(value === false || value === null || value === undefined) ? false : true}
        />
      )
    }
    else if(type === "DateTime"){
      if(value === null || value === "" || value === undefined){
        return(<td align="center" style={{color: "black", fontSize: 12}}>-</td>)
      }
      else{
        // console.log("ITEM", dataItem, value)
        let Date = value.substring(0, 10)
        let Time = value.substring(11, 16)
        return Date + " " + Time
      }
    }
    else{
      if(value === null || value === "" || value === undefined){
        return(<td align="center" style={{color: "black", fontSize: 12}}>-</td>)
      }
      else{
        return(<td>{value}</td>)
      }
    }
  }
  function checkToShowSection(section){
    let showSection = false
    for(let i=0; i<section.contents.length; i++){
      if(section.contents[i].show === true){
        showSection = true 
        break
      }
    }
    return showSection
  }
  function checkSectionOnLastChild(i){
    let lastSection = true
    if(i === gridForm.sections.length-1){ // The last section
      return true
    }
    else{
      let nextS = i+1
      // console.log("IT", i, gridForm.sections[nextS])
      for(let s=nextS; s<gridForm.sections.length; s++){
        for(let c=0; c<gridForm.sections[s].contents.length; c++){ // Check next section
          if(gridForm.sections[s].contents[c].show === true){
            lastSection = false
            break
          }
        }
      }
    }
    return lastSection
  }
  function getEnumLabel(name, id){
    for(let i=0; i<enumData.length; i++){
      if(enumData[i].name === name){
        for(let d=0; d<enumData[i].data.length; d++){
          if(enumData[i].data[d].id === id){
            return enumData[i].data[d].label
          }
        }
      }
    }
  }
  function dynamicSort(property, sortOrder, type) {
    if(type === "DateTime" || type === "Bool"){
      sortOrder = sortOrder * -1
    }
    if(type === "DateTime"){
      return function(a, b){
        if(a[property] !== null && b[property] !== null){
          let dateA = new Date(a[property].substring(0, 19))
          let timeInSecA =  dateA.getTime()/1000
          // console.log("timeInSecA", timeInSecA)
          let dateB = new Date(b[property].substring(0, 19))
          let timeInSecB =  dateB.getTime()/1000
          // console.log("timeInSecB", timeInSecB)
          var result = (timeInSecA < timeInSecB) ? -1 : (timeInSecA > timeInSecB) ? 1 : 0
          return result * sortOrder
        }
        else{
          if(a[property] === null){
            return -1 * sortOrder
          }
          return 1 * sortOrder
        }
      }
    }
    else if(type === "Int" || type === "Text" || type === "Float" || type === "Bool"){
      return function(a, b){
        var result = (a[property] < b[property]) ? -1 : (a[property] > b[property]) ? 1 : 0
        return result * sortOrder
      }
    }
    else if(type === "Enum"){
      return function(a, b){
        if(a[property] === null){
          return 1 * sortOrder
        }
        else{
          let labelA = getEnumLabel(property, a[property])
          // console.log("A", property, a[property], labelA)
          let labelB = getEnumLabel(property, b[property])
          // console.log("labelB", labelB, property, b)
          var result = (labelA < labelB) ? -1 : (labelA > labelB) ? 1 : 0
          return result * sortOrder
        }
        
      }
    }
  }
  function sortDataByColumn(name, type){
    let sortOrder = 1
    if(sortedColumn === name){
      sortOrder = sortedColumnOrder * -1
    }
    setSortedColumnOrder(sortOrder)
    setSortedColumn(name)
    let sortedDocList = filteredDocList.sort(dynamicSort(name, sortOrder, type))
    setPage(1)
    let fetchFrom = 0
    let fetchTo = size-1
    setFilteredDocList(sortedDocList)
    setDocList(sortedDocList)
    // fetchBySize(fetchFrom, fetchTo, sortedDocList)
  }
  //Отрисовка
  if(updateState !== null){
    try{
      return(
        docList !== null && gridForm !== null &&
        <Grid
          container
          direction="row"
          justifyContent="flex-start"
          alignItems="flex-start"
        >
          <Grid item xs={9}>
            <div style={{height: "400px", overflow: "scroll"}}>
              <table>
                <thead style={{backgroundColor: "#cfd8dc"}}>
                  {/* 1st Row Sections Labels */}
                  <tr>
                    {gridForm.sections.map((section, index) => {
                      let showSection = checkToShowSection(section)
                      if(showSection === true){
                        return (
                          <td
                            colSpan={section.contents.length}
                          >{section.label}</td>
                        )
                      }
                    })}
                  </tr>
                  {/* 2nd Row Sections Contents labels */}
                  <tr>
                    {gridForm.sections.map(section =>
                      section.contents.map(contentItem => {
                        if(contentItem.show === true){
                          return (
                            <td 
                              class="td-instr-head-style"
                              rowSpan="2"
                              onClick={()=> sortDataByColumn(contentItem.name, contentItem.type)} 
                            >
                              {contentItem.label}
                              {sortedColumn === contentItem.name ? sortedColumnOrder === 1 ? <ArrowDropDownIcon style={{marginBottom: -8}}/> : <ArrowDropUpIcon style={{marginBottom: -8}}/> : ""}
                            </td>
                          )
                        }
                      })
                    )}
                  </tr>
                </thead>
                <tbody>
                  {Object.keys(docList).length !== 0 &&
                    docList.map(dataItem => (
                      <tr key={keyGen(5)} style={{"height": 30}}>
                        {gridForm.sections.map(section => (
                          section.contents.map(contentItem => {
                            if(contentItem.show === true){
                              return(
                                <td key={keyGen(4)} align="left" style={{color: "black", fontSize: 12, "text-align": "center", "border-bottom": "1px solid grey"}}>
                                  {getGridFormItems(dataItem, contentItem)}
                                </td>
                              )
                            }
                          })
                        ))}
                      </tr>
                    )
                  )}                    
                </tbody>
              </table> 
            </div> 
          </Grid>
        </Grid>             
      )
    }
    catch(er){
      console.log("ERROR", er)
      return <LinearProgress/>
    }
  }
};

