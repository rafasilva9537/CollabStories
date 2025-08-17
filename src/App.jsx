import { BrowserRouter, Routes, Route } from "react-router-dom";

import './styles/Global.css'

import Home from './pages/home/Home'
import Story from './pages/stories/Story'

function App() {

  return (

    <BrowserRouter>
    
      <Routes>

        <Route path="/home" element={<Home/>}/>
        <Route path="/" element={<Story/>}/>

      </Routes>

    </BrowserRouter>

  )
}

export default App;