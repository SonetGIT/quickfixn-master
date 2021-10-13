import React, { useState, useEffect } from "react";
import MainForm from "./MainForm.jsx";
import Users from "./Users.jsx";

var fetch = require('node-fetch');
//ComponentManager(props) - родитель яв-ся Home
export default function ComponentManager(props) {
  const [kfbRESTApi] = useState(props.kfbRESTApi);
  const [task] = useState(props.task);
  const [taskVariables] = useState(props.taskVariables);
  const [userProfile] = useState(props.userProfile);
  
  useEffect(()=>{
    console.log("COMP MGR", props)
  },[])

  // GENERAL FUNCTIONS
  function parseFullDateTime(date){// "2017-05-24T10:30"
    console.log("IN DATE", date)
    if(date !== null){
      let newDate = new Date(date)
      // console.log("IN DATE", newDate)
      let dd = String(newDate.getDate()).padStart(2, '0')
      let mm = String(newDate.getMonth() + 1).padStart(2, '0') //January is 0!
      let yyyy = newDate.getFullYear()
      let hours = newDate.getHours()
      if(parseInt(hours) < 10){
        hours = "0" + hours
      }
      let minutes = newDate.getMinutes()
      if(parseInt(minutes) < 10){
        minutes = "0" + minutes
      }
      let fullDateTime = yyyy + '-' + mm + '-' + dd + "T" + hours + ":" + minutes // + ":" + seconds
      // console.log("FDATE", fullDateTime)
      return fullDateTime
    }
  }
  // Convert date to approptiate format
  function parseDate(date){
    try{
      let newDate = new Date(date) // "2020-01-26"
      let dd = String(newDate.getDate()).padStart(2, '0')
      let mm = String(newDate.getMonth() + 1).padStart(2, '0') //January is 0!
      let yyyy = newDate.getFullYear()
      let convertedDate = yyyy + '-' + mm + '-' + dd
      console.log("CDATE", convertedDate)
      return convertedDate
    }
    catch(er){
      return "NaN.NaN.NaN"
    }
  }
  function getCurrentFullDateTime(){
    var fullDate = parseDate(new Date())
    var hours = new Date().getHours()
    var minutes = new Date().getMinutes()
    var seconds = new Date().getSeconds()
    var convertedDate = fullDate + " " + hours + ":" + minutes + ":" + seconds + ".123456+06"
    return convertedDate
  }
  
  if(task === "showMainRefSearchForm"|| task === "showRefSearchForm" || task === "showEditRef" || task === "showCreateRef" ||
     task === "" || task === "" || task === ""){
     return (
      <MainForm
        // VARIABLES
        task={task}
        kfbRESTApi={kfbRESTApi}
        taskVariables={taskVariables}
        userProfile={userProfile}
        // FUNCTIONS
        setNewTask={props.setNewTask}
        parseFullDateTime={parseFullDateTime}
        getCurrentFullDateTime={getCurrentFullDateTime}
        parseDate={parseDate}
        getEnumData={props.getEnumData}
      />
    )
  }
  else if(task === "showUserSearchForm" || task === "showEditUserForm" || task === "showCreateUserForm"){
    return(
      <Users
        // VARIABLES
        task={task}
        kfbRESTApi={kfbRESTApi}
        taskVariables={taskVariables}
        userProfile={userProfile}
        // FUNCTIONS
        setNewTask={props.setNewTask}
        parseFullDateTime={parseFullDateTime}
        getCurrentFullDateTime={getCurrentFullDateTime}
        parseDate={parseDate}
        getEnumData={props.getEnumData}
      />
    )
  }
  // else if(task === "showUserReg")
  else{return <div></div>}
}