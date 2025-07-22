import { BrowserRouter, Route, Routes } from 'react-router-dom';
import '../public/globalStyles.css'
import './App.css'

import Register from '../src/pages/register/register'
import Login from '../src/pages/login/login'
import Home from '../src/pages/home/home'

function App() {

  return (
    <BrowserRouter>
      <Routes>

        <Route path='/register' element={<Register/>}/>
        <Route path='/login' element={<Login/>}/>
        <Route path='/' element={<Home/>}/>

      </Routes>
    </BrowserRouter>
  )
}

export default App;